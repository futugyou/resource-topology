
namespace KubeAgent.Monitor;

public class ReplicaSetMonitorMonitor(ILogger<ReplicaSetMonitorMonitor> logger, IKubernetes client, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.AppsV1.ListReplicaSetForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1ReplicaSet, V1ReplicaSetList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListReplicaSetForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
