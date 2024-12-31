
namespace KubeAgent.MonitorV2;

public class GeneralMonitor(ILogger<GeneralMonitor> logger, IKubernetes client) : IResourceMonitor
{
    public Task StartMonitoringAsync(MonitoredResource resource, CancellationToken cancellation)
    {
        using var childCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var childToken = childCts.Token;

        var resources = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(resource.KubeGroup, resource.KubeApiVersion, resource.KubePluralName,
        watch: true, allowWatchBookmarks: true, resourceVersion: resource.ResourceVersion, cancellationToken: cancellation);
        resources.Watch(
            onEvent: (Action<WatchEventType, object>)(async (type, item) =>
            {
                await OnEvent(resource.ReflectionType, type, item, cancellation);
            }),
            onError: (ex) =>
            {
                if (ex is KubernetesException kubernetesError)
                {
                    if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        throw ex;
                    }
                }
            },
            onClosed: () =>
            {
                logger.LogInformation("closed: {group} {version} {plural}", resource.KubeGroup, resource.KubeApiVersion, resource.KubePluralName);
            });

        return Task.CompletedTask;
    }

    private Task OnEvent(Type reflectionType, WatchEventType type, object item, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }

    public Task StopMonitoringAsync(string resourceId)
    {
        return Task.CompletedTask;
    }
}
