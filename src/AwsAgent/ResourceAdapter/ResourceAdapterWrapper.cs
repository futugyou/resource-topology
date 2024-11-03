
namespace AwsAgent.ResourceAdapter;

public class ResourceAdapterWrapper(IEnumerable<IResourceAdapter> adapters) : IResourceAdapterWrapper
{
    public async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
        var tasks = new List<Task<(List<Resource>, List<ResourceRelationship>)>>();
        foreach (var adapter in adapters)
        {
            tasks.Add(adapter.GetResourcAndRelationFromAWS(cancellation));
        }
        await Task.WhenAll(tasks);

        var resources = new List<Resource>();
        var ships = new List<ResourceRelationship>();
        foreach (var task in tasks)
        {
            var (resource, ship) = task.Result;
            resources.AddRange(resource);
            ships.AddRange(ship);
        }
        return (resources, ships);
    }
}