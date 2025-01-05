
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class JobMonitorMonitor(ILogger<JobMonitorMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.BatchV1.ListJobForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1Job, V1JobList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListJobForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}
