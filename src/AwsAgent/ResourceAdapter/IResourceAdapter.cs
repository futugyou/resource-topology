namespace AwsAgent.ResourceAdapter;

public interface IResourceAdapter
{
    Task<ResourceAndShip> GetResourcAndRelationFromAWS(CancellationToken cancellation);

    Task<ResourceAndShip> GetAdditionalResources(List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation)
    {
        return Task.FromResult(new ResourceAndShip(resources, ships));
    }

    Task<ResourceAndShip> MergeResources(List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation)
    {
        return Task.FromResult(new ResourceAndShip(resources, ships));
    }
}