
namespace KubeAgent.MonitorV2;

public class ResourceMonitorManager(IResourceDiscovery discovery, IResourceMonitor monitor) : IResourceMonitorManager
{
    private readonly HashSet<string> _currentMonitoredResourceIds = [];

    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await discovery.GetMonitoredResourcesAsync(cancellation);

        var resourcesToAdd = resources.Where(r => r.Operate == "add").ToList();
        var resourcesToDelete = resources.Where(r => r.Operate == "delete").ToList();
        var resourcesToRestart = resources.Where(r => r.Operate == "restart").ToList();

        foreach (var resource in resourcesToDelete)
        {
            await monitor.StopMonitoringAsync(resource.ID());
            _currentMonitoredResourceIds.Remove(resource.ID());
        }

        foreach (var resource in resourcesToRestart)
        {
            await monitor.StopMonitoringAsync(resource.ID());
            resource.Operate = "add";
            await monitor.StartMonitoringAsync(resource, cancellation);
            _currentMonitoredResourceIds.Add(resource.ID());
        }

        foreach (var resource in resourcesToAdd)
        {
            if (!_currentMonitoredResourceIds.Contains(resource.ID()))
            {
                await monitor.StartMonitoringAsync(resource, cancellation);
                _currentMonitoredResourceIds.Add(resource.ID());
            }
        }
    }
}