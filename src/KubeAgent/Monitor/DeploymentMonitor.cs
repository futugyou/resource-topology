
namespace KubeAgent.Monitor;

public class DeploymentMonitor(ILogger<DeploymentMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    readonly IResourceProcessor processor = factory.GetResourceProcessor();
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.AppsV1.ListDeploymentForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Deployment, V1DeploymentList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListDeploymentForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }

    private async Task HandlerResourceChange(WatchEventType type, V1Deployment item, CancellationToken cancellation)
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