namespace KubeAgent.Services;

public interface IDiscoveryProvider
{
    int Priority { get; }
    Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation);
}
