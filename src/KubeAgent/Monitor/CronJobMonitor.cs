
namespace KubeAgent.Monitor;

public class CronJobMonitorMonitor(ILogger<CronJobMonitorMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.BatchV1.ListCronJobForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1CronJob, V1CronJobList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListCronJobForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
