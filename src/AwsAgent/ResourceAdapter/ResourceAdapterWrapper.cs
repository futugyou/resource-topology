
namespace AwsAgent.ResourceAdapter;

public class ResourceAdapterWrapper(IEnumerable<IResourceAdapter> adapters, IOptions<ServiceOption> options) : IResourceAdapterWrapper
{
    public async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
        var resources = new ConcurrentBag<Resource>();
        var ships = new ConcurrentBag<ResourceRelationship>();
        var serviceOption = options.Value;
        using var limiter = new ConcurrencyLimiter(serviceOption?.MaxConcurrentAdapters ?? 5);
        var tasks = adapters.Select(adapter =>
            limiter.ExecuteAsync(async () =>
            {
                var (resource, ship) = await adapter.GetResourcAndRelationFromAWS(cancellation);
                resource.ForEach(r => resources.Add(r));
                ship.ForEach(r => ships.Add(r));
                return (resource, ship);
            })).ToList();

        await Task.WhenAll(tasks);

        return ([.. resources], [.. ships]);
    }
}
