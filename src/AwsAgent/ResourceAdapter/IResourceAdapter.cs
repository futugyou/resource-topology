namespace AwsAgent.ResourceAdapter;

public interface IResourceAdapter
{
    Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation);

    Task<(List<Resource>, List<ResourceRelationship>)> GetAdditionalResources(
        List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation)
    {
        return Task.FromResult((resources, ships));
    }

    Task<(List<Resource>, List<ResourceRelationship>)> MergeResources(
            List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation)
    {
        return Task.FromResult((resources, ships));
    }
}