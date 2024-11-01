
namespace AwsAgent.Infrastructure;

public class ResourceRelationshipRepository(MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor) : BaseRepository<ResourceRelationship>(client, optionsMonitor), IResourceRelationshipRepository
{
    public Task<bool> CreateResourceRelationships(List<ResourceRelationship> relationships, CancellationToken cancellation)
    {
        return CreateEntities(relationships, cancellation);
    }

    public Task<List<ResourceRelationship>> ListResourceRelationships(CancellationToken cancellation)
    {
        return ListEntities(cancellation);
    }
}