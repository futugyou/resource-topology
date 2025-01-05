using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class EndpointMonitor(ILogger<EndpointMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListEndpointsForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Endpoints, V1EndpointsList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListEndpointsForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
