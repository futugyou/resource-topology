namespace AwsAgent.ResourceAdapter;

public class ResourceAdapterWrapper(IEnumerable<IResourceAdapter> adapters, IOptions<ServiceOption> options) : IResourceAdapterWrapper
{
    private async Task<(List<Resource>, List<ResourceRelationship>)> ExecuteAdapterOperation(
        Func<IResourceAdapter, Task<(List<Resource>, List<ResourceRelationship>)>> operation,
        CancellationToken cancellation)
    {
        var resources = new ConcurrentBag<Resource>();
        var relationships = new ConcurrentBag<ResourceRelationship>();
        var serviceOption = options.Value;

        using var limiter = new ConcurrencyLimiter(serviceOption?.MaxConcurrentAdapters ?? 5);
        var tasks = adapters.Select(adapter =>
            limiter.ExecuteAsync(async () =>
            {
                var (resource, relationship) = await operation(adapter);
                resource.ForEach(r => resources.Add(r));
                relationship.ForEach(r => relationships.Add(r));
                return (resource, relationship);
            })).ToList();

        await Task.WhenAll(tasks).WaitAsync(cancellation);

        return ([.. resources], [.. relationships]);
    }

    private Task<(List<Resource>, List<ResourceRelationship>)> GetAdditionalResources(List<Resource> resources, List<ResourceRelationship> relationships, CancellationToken cancellation)
    {
        return ExecuteAdapterOperation(adapter => adapter.GetAdditionalResources(resources, relationships, cancellation), cancellation);
    }

    public async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
        (List<Resource> resources, List<ResourceRelationship> resourceShips) = await ExecuteAdapterOperation(adapter => adapter.GetResourcAndRelationFromAWS(cancellation), cancellation);
        (resources, resourceShips) = await GetAdditionalResources(resources, resourceShips, cancellation);
        (resources, resourceShips) = await MergeResources(resources, resourceShips, cancellation);
        
        return (resources, resourceShips);
    }

    private Task<(List<Resource>, List<ResourceRelationship>)> MergeResources(List<Resource> resources, List<ResourceRelationship> relationships, CancellationToken cancellation)
    {
        return ExecuteAdapterOperation(adapter => adapter.MergeResources(resources, relationships, cancellation), cancellation);
    }
}
