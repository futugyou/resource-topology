
namespace KubeAgent.Monitor;

public class NamespaceMonitor(ILogger<NamespaceMonitor> logger, IKubernetes client, ProcessorFactory factory) : IResourceMonitor
{
    readonly IResourceProcessor processor = factory.GetResourceProcessor();
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var namespaces = await client.CoreV1.ListNamespaceWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        namespaces.Watch<V1Namespace, V1NamespaceList>(onEvent: async (type, item) => await HandlerNamespaceChange(type, item, cancellation));
    }

    private async Task HandlerNamespaceChange(WatchEventType type, V1Namespace item, CancellationToken cancellation)
    {
        logger.LogInformation("namespace - {type} - {name} - {time}", type, item.Name(), DateTimeOffset.Now);
        var res = new Resource { ResourceType = "Namespace", Name = item.Name() };
        await processor.CollectingData(res, cancellation);
    }
}