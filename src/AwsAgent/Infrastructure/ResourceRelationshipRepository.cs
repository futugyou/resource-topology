
namespace AwsAgent.Infrastructure;

public class ResourceRelationshipRepository(ILogger<ResourceRelationshipRepository> logger, MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor) : BaseRepository<ResourceRelationship>(logger, client, optionsMonitor), IResourceRelationshipRepository
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