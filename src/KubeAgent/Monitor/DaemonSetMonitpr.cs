
namespace KubeAgent.Monitor;

public class DaemonSetMonitpr(ILogger<NodeMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.AppsV1.ListDaemonSetForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1DaemonSet, V1DaemonSetList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListDaemonSetForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}