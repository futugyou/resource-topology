using KubeAgent.ProcessorV2;

namespace KubeAgent.MonitorV2;

public class GeneralMonitor(ILogger<GeneralMonitor> logger, IKubernetes client,
                            IAdditionResourceProvider additionProvider,
                            [FromKeyedServices("General")] IDataProcessor<Resource> processor,
                            IRestartResourceTracker restartResourceTracker)
: IResourceMonitor, IDisposable
{
    readonly Dictionary<string, InternalWatcherInfo> watcherList = [];
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(45);
    private readonly TimeSpan _inactiveThreshold = TimeSpan.FromMinutes(9);
    int timerstart = 0;
    static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private void StartInactiveCheckTask(CancellationToken cancellation)
    {
        if (Interlocked.CompareExchange(ref timerstart, 1, 0) == 0)
        {
            Task.Run(async () =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(_checkInterval, cancellation);
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

            if (now - watcher.LastActiveTime > _inactiveThreshold)
            {
                logger.MonitorTimeout(watcher.Resource.ResourceId);
                await RestartResource(watcher.Resource, cancellation);
            }
        }
    }

    private async Task RestartResource(MonitoringContext context, CancellationToken cancellation)
    {
        await restartResourceTracker.AddRestartResource(new RestartContext { ResourceId = context.ResourceId }, cancellation);
    }

    public Task StartMonitoringAsync(MonitoringContext resource, CancellationToken cancellation)
    {
        StartInactiveCheckTask(cancellation);
        using var childCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var childToken = childCts.Token;

        var resources = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(resource.KubeGroup, resource.KubeApiVersion, resource.KubePluralName,
        watch: true, allowWatchBookmarks: true, resourceVersion: resource.ResourceVersion, cancellationToken: cancellation);
        var watcher = resources.Watch(
            onEvent: (Action<WatchEventType, object>)(async (type, item) =>
            {
                await OnEvent(resource, type, item, cancellation);
            }),
            onError: async (ex) =>
            {
                // From the code of KubernetesClient, 
                // The loop will stop watching when the stream reading fails. 
                // The stream reading is successful, but the watching will not be stopped when the processing fails.
                if (ex is KubernetesException kubernetesError)
                {
                    if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        // TODO: when to set resourceVersion?
                        resource.ResourceVersion = null;
                    }
                }

                logger.MonitorReceiveError(resource.ResourceId, ex);
                // TODO: It has not yet been determined which errors require a restart. 
                await RestartResource(resource, cancellation);
            },
            onClosed: () =>
            {
                logger.MonitorOnClosed(resource.ResourceId);
            });

        watcherList[resource.ResourceId] = new() { Watcher = watcher, Resource = resource, LastActiveTime = DateTime.Now };
        logger.MonitorAdded(resource.ResourceId);
        return Task.CompletedTask;
    }

    private async Task OnEvent(MonitoringContext resource, WatchEventType watchEventType, object item, CancellationToken cancellation)
    {
        if (watchEventType == WatchEventType.Error)
        {
            logger.MonitorOnEventError(resource.ResourceId);
            return;
        }

        try
        {
            var deserializedObject = DeserializeItem(item, resource.ReflectionType);

            if (deserializedObject is not IKubernetesObject<V1ObjectMeta> kubernetesObject)
            {
                logger.MonitorOnEventTypeError(resource.ResourceId);
                return;
            }

            LogEvent(watchEventType, resource);

            HandleWatcherList(resource, kubernetesObject, watchEventType);

            if (watchEventType == WatchEventType.Bookmark)
            {
                return;
            }

            if (kubernetesObject.Kind == "CustomResourceDefinition" && kubernetesObject is V1CustomResourceDefinition crd)
            {
                await HandleCustomResourceDefinition(crd, cancellation);
            }

            await ProcessResourceChange(kubernetesObject, watchEventType, cancellation);
        }
        catch (Exception ex)
        {
            logger.MonitorOnEventHandlingError(resource.ResourceId, ex);
        }
    }

    private static object? DeserializeItem(object item, Type targetType)
    {
        var json = JsonSerializer.Serialize(item, DefaultJsonSerializerOptions);
        return JsonSerializer.Deserialize(json, targetType);
    }

    private void LogEvent(WatchEventType watchEventType, MonitoringContext resource)
    {
        logger.MonitorOnEventProcessing(resource.ResourceId, watchEventType);
    }

    private void HandleWatcherList(MonitoringContext resource, IKubernetesObject<V1ObjectMeta> kubernetesObject, WatchEventType watchEventType)
    {
        if (watcherList.TryGetValue(resource.ResourceId, out var watcher))
        {
            resource.ResourceVersion = kubernetesObject.ResourceVersion();
            watcher.Resource = resource;

            if (watchEventType != WatchEventType.Bookmark)
            {
                watcher.LastActiveTime = DateTime.Now;
            }
        }
    }

    private async Task HandleCustomResourceDefinition(V1CustomResourceDefinition crd, CancellationToken cancellation)
    {
        if (crd?.Spec?.Versions == null || !crd.Spec.Versions.Any())
        {
            return;
        }

        foreach (var version in crd.Spec.Versions)
        {
            var monitoredResource = new MonitoredResource
            {
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

    private async Task ProcessResourceChange(IKubernetesObject<V1ObjectMeta> kubernetesObject, WatchEventType watchEventType, CancellationToken cancellation)
    {
        var jsonItem = kubernetesObject as dynamic;

        var resource = new Resource
        {
            ApiVersion = kubernetesObject.ApiVersion,
            Kind = kubernetesObject.Kind,
            Name = kubernetesObject.Name(),
            UID = kubernetesObject.Uid(),
            Configuration = JsonSerializer.Serialize(jsonItem, DefaultJsonSerializerOptions),
            Operate = watchEventType.ToString(),
        };

        await processor.CollectingData(resource, cancellation);
    }


    public Task StopMonitoringAsync(string resourceId)
    {
        if (watcherList.TryGetValue(resourceId, out var watcherInfo))
        {
            watcherInfo.Watcher?.Dispose();
            watcherList.Remove(resourceId);
            logger.MonitorStoped(resourceId);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<WatcherInfo>> GetWatcherListAsync(CancellationToken cancellation)
    {
        var list = watcherList.Values.Where(p => p.Resource != null).Select(watcher => new WatcherInfo
        {
            ResourceId = watcher.Resource!.ResourceId,
            KubeApiVersion = watcher.Resource!.KubeApiVersion,
            KubeKind = watcher.Resource!.KubeKind,
            KubeGroup = watcher.Resource!.KubeGroup,
            KubePluralName = watcher.Resource!.KubePluralName,
            ReflectionType = watcher.Resource!.ReflectionType,
            LastActiveTime = watcher.LastActiveTime,
        });

        return Task.FromResult(list);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var watcher in watcherList.Values)
            {
                watcher.Watcher?.Dispose();
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
