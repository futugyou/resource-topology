namespace KubeAgent.Discovery;

public class AdditionDiscoveryProvider(ILogger<AdditionDiscoveryProvider> logger) : AbstractChannelProcessor<MonitoredResource>, IDiscoveryProvider, IAdditionResourceProvider
{
    readonly Dictionary<string, MonitoredResource> monitoredResourceList = [];
    public int Priority => int.MaxValue;

    public Task AddAdditionResource(MonitoredResource resource, CancellationToken cancellation)
    {
        logger.AdditionResourceAdded(resource.ID());
        return CollectingData(resource, cancellation);
    }

    public async Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation)
    {
        await ProcessingData(cancellation);
        return monitoredResourceList.Values
        .Select(r =>
        {
            if (string.IsNullOrEmpty(r.Source))
            {
                r.Source = nameof(AdditionDiscoveryProvider);
            }
            return r;
        });
    }

    public Task RemoveAdditionResource(string resourceId, CancellationToken cancellation)
    {
        monitoredResourceList.Remove(resourceId);
        return Task.CompletedTask;
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
