namespace KubeAgent.Discovery;

public class AdditionDiscoveryProvider() : ProcessorV2.AbstractChannelProcessor<MonitoredResource>, IDiscoveryProvider, IAdditionResourceProvider
{
    readonly Dictionary<string, MonitoredResource> monitoredResourceList = [];
    public int Priority => int.MaxValue;

    public Task AddAdditionResource(MonitoredResource resource, CancellationToken cancellation)
    {
        return CollectingData(resource, cancellation);
    }

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
