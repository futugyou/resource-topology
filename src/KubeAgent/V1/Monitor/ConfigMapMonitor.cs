
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class ConfigMapMonitor(ILogger<ConfigMapMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListConfigMapForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1ConfigMap, V1ConfigMapList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListConfigMapForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
