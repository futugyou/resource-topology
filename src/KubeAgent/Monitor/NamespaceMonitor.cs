
namespace KubeAgent.Monitor;

public class NamespaceMonitor(ILogger<NamespaceMonitor> logger, IKubernetes client, ProcessorFactory factory) : IResourceMonitor
{
    readonly IResourceProcessor processor = factory.GetResourceProcessor();
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.CoreV1.ListNamespaceWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Namespace, V1NamespaceList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: (ex) => logger.LogError("ListNamespaceWithHttpMessagesAsync error: {ex}", ex.Message));
    }

    private async Task HandlerResourceChange(WatchEventType type, V1Namespace item, CancellationToken cancellation)
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