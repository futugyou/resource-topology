
namespace KubeAgent.Monitor;

public class ServiceMonitor(ILogger<ServiceMonitor> logger, IKubernetes client, IResourceProcessor resourceProcessor) : IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var services = await client.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        services.Watch<V1Service, V1ServiceList>(onEvent: async (type, item) => await HandlerServiceChange(type, item, cancellation));
    }

    private async Task HandlerServiceChange(WatchEventType type, V1Service item, CancellationToken cancellation)
    {
        logger.LogInformation("service - {type} - {name} - {time}", type, item.Name(), DateTimeOffset.Now);
        var res = new Resource { ResourceType = "Service", Name = item.Name() };
        await resourceProcessor.CollectingData(res, cancellation);
    }
}