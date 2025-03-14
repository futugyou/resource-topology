using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class StorageClassMonitor(ILogger<StorageClassMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.StorageV1.ListStorageClassWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1StorageClass, V1StorageClassList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListStorageClassWithHttpMessagesAsync", ex, cancellation));
    }
}
