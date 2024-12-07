
namespace KubeAgent.Monitor;

public class ConfigMapMonitor(ILogger<NodeMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListConfigMapForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1ConfigMap, V1ConfigMapList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListStatefulSetForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}