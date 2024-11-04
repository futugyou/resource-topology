
namespace AwsAgent.ResourceAdapter;

public class ResourceAdapterWrapper(IEnumerable<IResourceAdapter> adapters) : IResourceAdapterWrapper
{
    public async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
       var tasks = adapters.Select(adapter => adapter.GetResourcAndRelationFromAWS(cancellation)).ToList();
        
        // Await all tasks to complete
        var results = await Task.WhenAll(tasks);

        var resources = new List<Resource>();
        var ships = new List<ResourceRelationship>();
        
        foreach (var (resource, ship) in results)
        {
            resources.AddRange(resource);
            ships.AddRange(ship);
        }

        return (resources, ships);
    }
}