using KubeAgent.ProcessorV2;

namespace KubeAgent.MonitorV2;

public class GeneralMonitor(ILogger<GeneralMonitor> logger, IKubernetes client,
 [FromKeyedServices("Custom")] IDataProcessor<MonitoredResource> rewatchProcessor, [FromKeyedServices("General")] IDataProcessor<Resource> processor)
: IResourceMonitor
{
    readonly Dictionary<string, WatcherInfo> watcherList = [];
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(45);
    private readonly TimeSpan _inactiveThreshold = TimeSpan.FromMinutes(9);
    int timerstart = 0;

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
                await RestartResource(watcher.Resource, cancellation);
            }
        }
    }

    private async Task RestartResource(MonitoredResource resource, CancellationToken cancellation)
    {
        resource.Operate = "restart";
        await rewatchProcessor.CollectingData(resource, cancellation);
    }

    public Task StartMonitoringAsync(MonitoredResource resource, CancellationToken cancellation)
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

                // TODO: It has not yet been determined which errors require a restart. 
                await RestartResource(resource, cancellation);
            },
            onClosed: () =>
            {
                logger.LogInformation("closed: {group} {version} {plural}", resource.KubeGroup, resource.KubeApiVersion, resource.KubePluralName);
            });

        watcherList[resource.ID()] = new() { Watcher = watcher, Resource = resource, LastActiveTime = DateTime.Now }; ;
        return Task.CompletedTask;
    }

    private async Task OnEvent(MonitoredResource resource, WatchEventType watchEventType, object item, CancellationToken cancellation)
    {
        try
        {
            var deserializedObject = DeserializeItem(item, resource.ReflectionType);

            if (deserializedObject is not IKubernetesObject<V1ObjectMeta> kubernetesObject)
            {
                return;
            }

            LogEvent(watchEventType, kubernetesObject);

            if (watchEventType == WatchEventType.Error)
            {
                return;
            }

            HandleWatcherList(resource, kubernetesObject, watchEventType);

            if (kubernetesObject.Kind == "CustomResourceDefinition" && kubernetesObject is V1CustomResourceDefinition crd)
            {
                await HandleCustomResourceDefinition(crd, cancellation);
            }

            await ProcessResourceChange(item, kubernetesObject, watchEventType, cancellation);
        }
        catch (Exception ex)
        {
            logger.LogError("OnEvent error: {error}", (ex.InnerException ?? ex).Message);
        }
    }

    private static object? DeserializeItem(object item, Type targetType)
    {
        var json = JsonSerializer.Serialize(item);
        return JsonSerializer.Deserialize(json, targetType);
    }

    private void LogEvent(WatchEventType watchEventType, IKubernetesObject<V1ObjectMeta> kubernetesObject)
    {
        logger.LogInformation("event: {type} {kind} {name}", watchEventType, kubernetesObject.Kind, kubernetesObject.Name());
    }

    private void HandleWatcherList(MonitoredResource resource, IKubernetesObject<V1ObjectMeta> kubernetesObject, WatchEventType watchEventType)
    {
        if (watcherList.TryGetValue(resource.ID(), out var watcher))
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
        foreach (var version in crd.Spec.Versions)
        {
            var monitoredResource = new MonitoredResource
            {
                KubeApiVersion = version.Name,
                KubeKind = crd.Spec.Names.Kind,
                KubeGroup = crd.Spec.Group,
                KubePluralName = crd.Spec.Names.Plural,
                ResourceVersion = null,
                Operate = "add",
                ReflectionType = typeof(GeneralCustomResource),
            };

            await rewatchProcessor.CollectingData(monitoredResource, cancellation);
        }
    }

    private async Task ProcessResourceChange(object item, IKubernetesObject<V1ObjectMeta> kubernetesObject, WatchEventType watchEventType, CancellationToken cancellation)
    {
        var jsonItem = item as dynamic;

        var resource = new Resource
        {
            ApiVersion = kubernetesObject.ApiVersion,
            Kind = kubernetesObject.Kind,
            Name = kubernetesObject.Name(),
            UID = kubernetesObject.Uid(),
            Configuration = JsonSerializer.Serialize(jsonItem),
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
        }
        logger.LogInformation("stop monitoring: {resourceId}", resourceId);
        return Task.CompletedTask;
    }
}

public class WatcherInfo
{
    public MonitoredResource? Resource { get; set; }
    public Watcher<object>? Watcher { get; set; }
    public DateTime LastActiveTime { get; set; }
}