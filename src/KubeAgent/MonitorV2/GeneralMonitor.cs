
using KubeAgent.ProcessorV2;

namespace KubeAgent.MonitorV2;

public class GeneralMonitor(ILogger<GeneralMonitor> logger, IKubernetes client,
 [FromKeyedServices("Custom")] IDataProcessor<MonitoredResource> rewatchProcessor, [FromKeyedServices("General")] IDataProcessor<Resource> processor)
: IResourceMonitor
{
    readonly Dictionary<string, WatcherInfo> watcherList = [];
    public Task StartMonitoringAsync(MonitoredResource resource, CancellationToken cancellation)
    {
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
                resource.Operate = "restart";
                await rewatchProcessor.CollectingData(resource, cancellation);
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
        var json = JsonSerializer.Serialize(item);
        object? deserializedObject = JsonSerializer.Deserialize(json, resource.ReflectionType);
        if (deserializedObject is IKubernetesObject<V1ObjectMeta> kubernetesObject)
        {
            logger.LogInformation("event: {type} {kind} {name}", watchEventType, kubernetesObject.Kind, kubernetesObject.Name());

            if (watchEventType == WatchEventType.Error)
            {
                return;
            }

            if (watchEventType == WatchEventType.Added ||
                        watchEventType == WatchEventType.Modified ||
                        watchEventType == WatchEventType.Deleted ||
                        watchEventType == WatchEventType.Bookmark)
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

            if (watchEventType == WatchEventType.Bookmark)
            {
                return;
            }

            if (kubernetesObject.Kind == "CustomResourceDefinition" && kubernetesObject is V1CustomResourceDefinition crd)
            {
                foreach (var version in crd.Spec.Versions)
                {
                    await rewatchProcessor.CollectingData(new MonitoredResource
                    {
                        KubeApiVersion = version.Name,
                        KubeKind = crd.Spec.Names.Kind,
                        KubeGroup = crd.Spec.Group,
                        KubePluralName = crd.Spec.Names.Plural,
                        ResourceVersion = null,
                        Operate = "add",
                        ReflectionType = typeof(GeneralCustomResource),
                    }, cancellation);
                }
            }

            var jsonItem = item as dynamic;

            try
            {
                var res = new Resource
                {
                    ApiVersion = kubernetesObject.ApiVersion,
                    Kind = kubernetesObject.Kind,
                    Name = kubernetesObject.Name(),
                    UID = kubernetesObject.Uid(),
                    Configuration = JsonSerializer.Serialize(jsonItem),
                    Operate = watchEventType.ToString(),
                };
                await processor.CollectingData(res, cancellation);
            }
            catch (Exception ex)
            {
                logger.LogError("HandlerResourceChange: {error}", (ex.InnerException ?? ex).Message);
            }
        }
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