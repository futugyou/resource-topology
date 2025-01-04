namespace KubeAgent.Discovery;

/// <summary>
/// IDiscoveryProvider provides multiple ways to obtain a list of resources that need to be monitored, mainly used by IDiscoveryProvider.
/// </summary>
public interface IDiscoveryProvider
{
    int Priority { get; }
    Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation);
}
