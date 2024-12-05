
namespace KubeAgent.Monitor;

public class ServiceMonitor(ILogger<ServiceMonitor> logger, IKubernetes client) : IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var services = await client.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        services.Watch<V1Service, V1ServiceList>(onEvent: HandlerServiceChange);
    }

    private void HandlerServiceChange(WatchEventType type, V1Service item)
    {
        logger.LogInformation("service - {type} - {name} - {time}", type, item.Name(), DateTimeOffset.Now);
    }
}