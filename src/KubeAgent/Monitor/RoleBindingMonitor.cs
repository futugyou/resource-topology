namespace KubeAgent.Monitor;

public class RoleBindingMonitor(ILogger<RoleBindingMonitor> logger, IKubernetes client, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.RbacAuthorizationV1.ListRoleBindingForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1RoleBinding, V1RoleBindingList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListRoleBindingForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
