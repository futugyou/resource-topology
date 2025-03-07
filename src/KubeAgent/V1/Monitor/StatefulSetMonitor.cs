
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class StatefulSetMonitor(ILogger<StatefulSetMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.AppsV1.ListStatefulSetForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1StatefulSet, V1StatefulSetList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListStatefulSetForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}