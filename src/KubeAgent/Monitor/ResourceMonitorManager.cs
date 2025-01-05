
namespace KubeAgent.Monitor;

public class ResourceMonitorManager(IResourceDiscovery discovery, IResourceMonitor monitor, IRestartResourceTracker restartResourceTracker) : IResourceMonitorManager
{

    public async Task MonitorResource(CancellationToken cancellation)
    {
        var currentList = await monitor.GetWatcherListAsync(cancellation);
        var currentIdList = currentList.Select(x => x.ResourceId).ToList();

        var resources = await discovery.GetMonitoredResourcesAsync(cancellation);
        var resourceIds = resources.Select(x => x.ID()).ToList();

        var resourcesToAdd = resources.Where(x => !currentIdList.Contains(x.ID())).ToList();
        var resourcesToDelete = currentList.Where(x => !resourceIds.Contains(x.ResourceId)).ToList();

        var resourcesToRestart = await restartResourceTracker.GetRestartResources(cancellation);

        foreach (var resource in resourcesToDelete)
        {
            await monitor.StopMonitoringAsync(resource.ResourceId);
        }

        foreach (var resource in resourcesToRestart)
        {
            var res = currentList.FirstOrDefault(x => x.ResourceId == resource.ResourceId);
            if (res != null)
            {
                await monitor.StopMonitoringAsync(res.ResourceId);
                var monitoringContext = new MonitoringContext
                {
                    ResourceId = res.ResourceId,
                    KubeApiVersion = res.KubeApiVersion,
                    KubeKind = res.KubeKind,
                    KubeGroup = res.KubeGroup,
                    KubePluralName = res.KubePluralName,
                    ReflectionType = res.ReflectionType
                };
                await monitor.StartMonitoringAsync(monitoringContext, cancellation);
            }
        }

        foreach (var resource in resourcesToAdd)
        {
            var monitoringContext = MonitoringContext.FromMonitoredResource(resource);
            await monitor.StartMonitoringAsync(monitoringContext, cancellation);
        }
    }
}