
namespace KubeAgent.MonitorV2;

public class ResourceMonitorManager(IResourceDiscovery discovery, IResourceMonitor monitor) : IResourceMonitorManager
{
    private readonly HashSet<string> _currentMonitoredResourceIds = [];
    
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await discovery.GetMonitoredResourcesAsync(cancellation);
        var resourceIds = new HashSet<string>(resources.Select(r => r.ID()));

        var newResources = resources.Where(r => !_currentMonitoredResourceIds.Contains(r.ID())).ToList();
        foreach (var resource in newResources)
        {
            await monitor.StartMonitoringAsync(resource);
            _currentMonitoredResourceIds.Add(resource.ID());
        }

        var removedResources = _currentMonitoredResourceIds.Except(resourceIds).ToList();
        foreach (var resourceId in removedResources)
        {
            await monitor.StopMonitoringAsync(resourceId);
            _currentMonitoredResourceIds.Remove(resourceId);
        }
    }
}