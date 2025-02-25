
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class NodeMonitor(ILogger<NodeMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    readonly IResourceProcessor processor = processor;
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListNodeWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Node, V1NodeList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListNodeWithHttpMessagesAsync", ex, cancellation));
    }

    private async Task HandlerResourceChange(WatchEventType type, V1Node item, CancellationToken cancellation)
    {
        var res = new Resource
        {
            ApiVersion = item.ApiVersion,
            Kind = item.Kind,
            Name = item.Name(),
            UID = item.Uid(),
            Configuration = JsonSerializer.Serialize(item),
            Operate = type.ToString(),
        };
        await processor.CollectingData(res, cancellation);
    }
}