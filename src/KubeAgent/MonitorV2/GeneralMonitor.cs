
using KubeAgent.ProcessorV2;

namespace KubeAgent.MonitorV2;

public class GeneralMonitor(ILogger<GeneralMonitor> logger, IKubernetes client,
 [FromKeyedServices("Custom")] IDataProcessor<MonitoredResource> crdProcessor, [FromKeyedServices("General")] IDataProcessor<Resource> processor)
: IResourceMonitor
{
    public Task StartMonitoringAsync(MonitoredResource resource, CancellationToken cancellation)
    {
        using var childCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var childToken = childCts.Token;

        var resources = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(resource.KubeGroup, resource.KubeApiVersion, resource.KubePluralName,
        watch: true, allowWatchBookmarks: true, resourceVersion: resource.ResourceVersion, cancellationToken: cancellation);
        resources.Watch(
            onEvent: (Action<WatchEventType, object>)(async (type, item) =>
            {
                await OnEvent(resource.ReflectionType, type, item, cancellation);
            }),
            onError: (ex) =>
            {
                if (ex is KubernetesException kubernetesError)
                {
                    if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        throw ex;
                    }
                }
            },
            onClosed: () =>
            {
                logger.LogInformation("closed: {group} {version} {plural}", resource.KubeGroup, resource.KubeApiVersion, resource.KubePluralName);
            });

        return Task.CompletedTask;
    }

    private async Task OnEvent(Type reflectionType, WatchEventType type, object item, CancellationToken cancellation)
    {
        var json = JsonSerializer.Serialize(item);
        object? deserializedObject = JsonSerializer.Deserialize(json, reflectionType);
        if (deserializedObject is IKubernetesObject<V1ObjectMeta> kubernetesObject)
        {
            if (type == WatchEventType.Bookmark)
            {
                return;
            }

            Console.WriteLine($"Event: {kubernetesObject.Kind} {kubernetesObject.ApiGroup()} {kubernetesObject.ApiVersion} {kubernetesObject.Name()}");
            if (kubernetesObject.Kind == "CustomResourceDefinition" && kubernetesObject is V1CustomResourceDefinition crd)
            {
                foreach (var version in crd.Spec.Versions)
                {
                    await crdProcessor.CollectingData(new MonitoredResource
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
                    Operate = type.ToString(),
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
        Console.WriteLine($"Event: {resourceId}  ");
        return Task.CompletedTask;
    }
}
