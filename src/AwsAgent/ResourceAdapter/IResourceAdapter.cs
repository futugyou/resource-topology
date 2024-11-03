namespace AwsAgent.ResourceAdapter;

public interface IResourceAdapter
{
    Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation);  
}