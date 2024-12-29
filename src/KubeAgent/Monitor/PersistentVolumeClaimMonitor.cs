namespace KubeAgent.Monitor;

public class PersistentVolumeClaimMonitor(ILogger<PersistentVolumeClaimMonitor> logger, IKubernetes client, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListPersistentVolumeClaimForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1PersistentVolumeClaim, V1PersistentVolumeClaimList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListPersistentVolumeClaimForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
