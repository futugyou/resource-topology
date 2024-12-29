namespace KubeAgent.Monitor;

public class ClusterRoleMonitor(ILogger<ClusterRoleMonitor> logger, IKubernetes client, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.RbacAuthorizationV1.ListClusterRoleWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1ClusterRole, V1ClusterRoleList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListClusterRoleWithHttpMessagesAsync", ex, cancellation));
    }
}
