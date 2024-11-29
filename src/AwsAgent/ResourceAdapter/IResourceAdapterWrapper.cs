namespace AwsAgent.ResourceAdapter;

public interface IResourceAdapterWrapper
{
    Task<ResourceAndShip> GetResourcAndRelationFromAWS(CancellationToken cancellation);
}