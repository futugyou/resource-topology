
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class ServiceMonitor(ILogger<ServiceMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    readonly IResourceProcessor processor = processor;
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Service, V1ServiceList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListServiceForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }

    private async Task HandlerResourceChange(WatchEventType type, V1Service item, CancellationToken cancellation)
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