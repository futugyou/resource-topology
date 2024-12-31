namespace KubeAgent.Services;

public class RewatchDiscoveryProvider() : ProcessorV2.AbstractChannelProcessor<MonitoredResource>, IDiscoveryProvider
{
    readonly Dictionary<string, MonitoredResource> monitoredResourceList = [];
    public int Priority => 2;

    public async Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation)
    {
        await ProcessingData(cancellation);
        return monitoredResourceList.Values;
    }

    override protected Task ProcessBatch(List<MonitoredResource> batch, CancellationToken cancellation)
    {
        foreach (var resource in batch)
        {
            monitoredResourceList[resource.ID()] = resource;
        }

        return Task.CompletedTask;
    }
}
