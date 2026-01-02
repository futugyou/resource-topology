namespace KubeAgent.Monitor;

public class GeneralMonitor(ILogger<GeneralMonitor> logger,
                            IKubernetesClientProvider clientProvider,
                            IAdditionResourceProvider additionProvider,
                            [FromKeyedServices("General")] IDataProcessor<Resource> processor,
                            IRestartResourceTracker restartResourceTracker,
                            IMapper mapper,
                            IOptionsMonitor<MonitorOptions> monitorOptions,
                            ISerializer serializer) : IResourceMonitor, IDisposable
{
    readonly ConcurrentDictionary<string, InternalWatcherInfo> watcherList = [];
    readonly ConcurrentDictionary<string, int> clientCounter = [];

    int timerstart = 0;

    private void StartInactiveCheckTask(CancellationToken cancellation)
    {
        if (Interlocked.CompareExchange(ref timerstart, 1, 0) == 0)
        {
            Task.Run(async () =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(monitorOptions.CurrentValue.CheckIntervalSeconds), cancellation);
                    CheckInactiveResources(cancellation);
                }
            }, cancellation);
        }
    }

    // async void will not warn the caller if the caller does not await the return value, async Task will.
    private async void CheckInactiveResources(CancellationToken cancellation)
    {
        var now = DateTime.Now;
        foreach (var watcher in watcherList.Values)
        {
            if (watcher == null || watcher.Resource == null)
            {
                continue;
            }

            if (now - watcher.LastActiveTime > TimeSpan.FromMinutes(monitorOptions.CurrentValue.InactiveThresholdMinutes))
            {
                var resourceId = watcher.Resource.ResourceId();
                logger.MonitorTimeout(resourceId);
                await RestartResource(watcher.Resource, cancellation);
            }
        }
    }

    private async Task RestartResource(MonitoringContext context, CancellationToken cancellation)
    {
        await restartResourceTracker.AddRestartResource(new RestartContext { ResourceId = context.ResourceId() }, cancellation);
    }

    public async Task StartMonitoringAsync(MonitoringContext resource, CancellationToken cancellation)
    {
        var resourceId = resource.ResourceId();
        if (watcherList.ContainsKey(resourceId))
        {
            logger.MonitorAlreadyExist(resourceId);
            return;
        }

        StartInactiveCheckTask(cancellation);
        using var childCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var childToken = childCts.Token;

        var clientList = await clientProvider.GetKubernetesClientsAsync(cancellation);
        if (!clientList.TryGetValue(resource.ClusterName, out var client))
        {
            logger.ClientNotFound(resource.ClusterName);
            return;
        }

        clientCounter.AddOrUpdate(resourceId, 1, (k, v) => v + 1);

        // Extracted the common resource creation logic into a helper method
        Watcher<object>? resources = await CreateWatcher(client, resource, cancellation);

        if (resources != null)
        {
            watcherList[resourceId] = new() { Watcher = resources, Resource = resource, LastActiveTime = DateTime.Now };
            logger.MonitorAdded(resourceId);
        }
    }

    private async Task<Watcher<object>?> CreateWatcher(IKubernetes client, MonitoringContext resource, CancellationToken cancellation)
    {
        var resourceId = resource.ResourceId();
        async void onEventHandler(WatchEventType type, object item)
        {
            await OnEvent(resource, type, item, cancellation);
        }

        async void onErrorHandler(Exception ex)
        {
            if (ex is KubernetesException kubernetesError)
            {
                if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                {
                    resource.ResourceVersion = null;
                }
            }

            logger.MonitorReceiveError(resourceId, ex);
            await RestartResource(resource, cancellation);
        }

        void onClosedHandler()
        {
            logger.MonitorOnClosed(resourceId);
        }

        if (string.IsNullOrWhiteSpace(resource.Namespace))
        {
            return client.CustomObjects.WatchListClusterCustomObject(
                group: resource.KubeGroup,
                version: resource.KubeApiVersion,
                plural: resource.KubePluralName,
                onEvent: onEventHandler,
                onError: onErrorHandler,
                onClosed: onClosedHandler,
                allowWatchBookmarks: true,
                resourceVersion: resource.ResourceVersion);
        }
        else
        {
            return client.CustomObjects.WatchListNamespacedCustomObject(
                group: resource.KubeGroup,
                version: resource.KubeApiVersion,
                namespaceParameter: resource.Namespace,
                plural: resource.KubePluralName,
                onEvent: onEventHandler,
                onError: onErrorHandler,
                onClosed: onClosedHandler,
                allowWatchBookmarks: true,
                resourceVersion: resource.ResourceVersion);
        }
    }

    private async Task OnEvent(MonitoringContext resource, WatchEventType watchEventType, object item, CancellationToken cancellation)
    {
        var resourceId = resource.ResourceId();
        if (watchEventType == WatchEventType.Error)
        {
            logger.MonitorOnEventError(resourceId);
            return;
        }

        try
        {
            // item.GetType() shows System.Text.Json.JsonElement
            // so we need serializer.Deserialize
            var json = serializer.Serialize(item);
            var deserializedObject = serializer.Deserialize(json, resource.ReflectionType);

            if (deserializedObject is not IKubernetesObject<V1ObjectMeta> kubernetesObject)
            {
                // Normally, it will not run to this point. If the type is incorrect, an error will be reported during serialization.
                // If it really runs to this point, I would be very curious about what kind of data this is.
                logger.MonitorOnEventTypeError(resourceId);
                return;
            }

            logger.MonitorOnEventProcessing(resourceId, watchEventType);

            HandleWatcherList(resource, kubernetesObject, watchEventType);

            if (watchEventType == WatchEventType.Bookmark)
            {
                return;
            }

            if (monitorOptions.CurrentValue.CustomResourced && kubernetesObject.Kind == "CustomResourceDefinition"
            && kubernetesObject is V1CustomResourceDefinition crd)
            {
                await HandleCustomResourceDefinition(crd, resource, cancellation);
            }

            await ProcessResourceChange(kubernetesObject, watchEventType, json, resource, cancellation);
        }
        catch (Exception ex)
        {
            logger.MonitorOnEventHandlingError(resourceId, ex);
        }
    }

    private void HandleWatcherList(MonitoringContext resource, IKubernetesObject<V1ObjectMeta> kubernetesObject, WatchEventType watchEventType)
    {
        if (watcherList.TryGetValue(resource.ResourceId(), out var watcher))
        {
            resource.ResourceVersion = kubernetesObject.ResourceVersion();
            watcher.Resource = resource;

            if (watchEventType != WatchEventType.Bookmark)
            {
                watcher.LastActiveTime = DateTime.Now;
            }
        }
    }

    private async Task HandleCustomResourceDefinition(V1CustomResourceDefinition crd, MonitoringContext resource, CancellationToken cancellation)
    {
        if (crd?.Spec?.Versions == null || !crd.Spec.Versions.Any())
        {
            return;
        }

        foreach (var version in crd.Spec.Versions)
        {
            var monitoredResource = new MonitoredResource
            {
                ClusterName = resource.ClusterName,
                KubeApiVersion = version.Name,
                KubeKind = crd.Spec.Names.Kind,
                KubeGroup = crd.Spec.Group,
                KubePluralName = crd.Spec.Names.Plural,
                Source = nameof(GeneralMonitor),
                ReflectionType = typeof(GeneralCustomResource),
            };

            await additionProvider.AddAdditionResource(monitoredResource, cancellation);
        }
    }

    private async Task ProcessResourceChange(IKubernetesObject<V1ObjectMeta> kubernetesObject, WatchEventType watchEventType, string config, MonitoringContext context, CancellationToken cancellation)
    {
        var resource = new Resource
        {
            Cluster = context.ClusterName,
            ApiVersion = kubernetesObject.ApiVersion,
            Kind = kubernetesObject.Kind,
            Name = kubernetesObject.Name(),
            UID = kubernetesObject.Uid(),
            Configuration = config,
            Operate = watchEventType.ToString(),
            ResourceCreationTime = kubernetesObject.CreationTimestamp().GetValueOrDefault(),
        };

        if (kubernetesObject.OwnerReferences() != null && kubernetesObject.OwnerReferences().Any())
        {
            resource.OwnerReferences = [.. kubernetesObject.OwnerReferences().Select(owner =>
            {
                return new OwnerReference
                {
                    ApiVersion = owner.ApiVersion,
                    Kind = owner.Kind,
                    Name = owner.Name,
                    UID = owner.Uid,
                };
            })];
        }

        if (kubernetesObject.Labels() != null && kubernetesObject.Labels().Any())
        {
            foreach (var label in kubernetesObject.Labels())
            {
                resource.Tags.Add(label.Key, label.Value);
            }
        }

        if (kubernetesObject.Annotations() != null && kubernetesObject.Annotations().Any())
        {
            foreach (var annotation in kubernetesObject.Annotations())
            {
                resource.Tags.Add(annotation.Key, annotation.Value);
            }
        }

        if (resource.Tags.TryGetValue("topology.kubernetes.io/region", out string? region))
        {
            resource.Region = region;
        }

        if (resource.Tags.TryGetValue("topology.kubernetes.io/zone", out string? zone))
        {
            resource.Zone = zone;
        }

        await processor.CollectingData(resource, cancellation);
    }

    public async Task StopMonitoringAsync(string resourceId)
    {
        if (watcherList.Remove(resourceId, out var watcherInfo))
        {
            watcherInfo.Watcher?.Dispose();
            logger.MonitorStoped(resourceId);

            if (watcherInfo.Resource == null)
            {
                return;
            }

            var clusterName = watcherInfo.Resource.ClusterName;
            clientCounter.AddOrUpdate(clusterName, 0, (k, v) => v > 1 ? v - 1 : 0);
            if (clientCounter[clusterName] == 0)
            {
                clientCounter.TryRemove(clusterName, out _);
                await clientProvider.ReleaseClientAsync(clusterName, CancellationToken.None);
            }
        }

        return;
    }

    public Task<IEnumerable<WatcherInfo>> GetWatcherListAsync(CancellationToken cancellation)
    {
        var list = watcherList.Values.Where(p => p.Resource != null).Select(watcher =>
        {
            var info = mapper.Map<WatcherInfo>(watcher.Resource!);
            info.LastActiveTime = watcher.LastActiveTime;
            return info;
        });

        return Task.FromResult(list);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var key in watcherList.Keys.ToList())
            {
                if (watcherList.Remove(key, out var watcher))
                {
                    watcher.Watcher?.Dispose();
                }
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public class InternalWatcherInfo
    {
        public MonitoringContext? Resource { get; set; }
        public Watcher<object>? Watcher { get; set; }
        public DateTime LastActiveTime { get; set; }
    }
}
