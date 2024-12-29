
namespace KubeAgent.Monitor;

public class IngressMonitor(ILogger<IngressMonitor> logger, IKubernetes client, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.NetworkingV1.ListIngressForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Ingress, V1IngressList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListIngressForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}