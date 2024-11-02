namespace AwsAgent.ResourceAdapter;

public interface IAMResourceAdapter
{
    Task<(List<Resource>, List<ResourceRelationship>)> ConvertIAMToResource(CancellationToken cancellation);
    Task<(List<Resource>, List<ResourceRelationship>)> ConvertConfigToResource(CancellationToken cancellation);
}
