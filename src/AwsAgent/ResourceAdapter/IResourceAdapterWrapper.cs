namespace AwsAgent.ResourceAdapter;

public interface IResourceAdapterWrapper
{
    Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation);
}