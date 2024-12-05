
namespace KubeAgent.Monitor;

public class NamespaceMonitor(ILogger<NamespaceMonitor> logger, IKubernetes client) : IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var namespaces = await client.CoreV1.ListNamespaceWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        namespaces.Watch<V1Namespace, V1NamespaceList>(onEvent: HandlerNamespaceChange);
    }

    private void HandlerNamespaceChange(WatchEventType type, V1Namespace item)
    {
        logger.LogInformation("namespace - {type} - {name} - {time}", type, item.Name(), DateTimeOffset.Now);
    }
}