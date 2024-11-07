namespace AwsAgent.ResourceAdapter;

public interface IResourceAdapterWrapper
{
    Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation);

    Task<(List<Resource>, List<ResourceRelationship>)> GetAdditionalResources(List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation);

    Task<(List<Resource>, List<ResourceRelationship>)> MergeResources(List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation);
}