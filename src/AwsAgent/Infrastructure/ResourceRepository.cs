namespace AwsAgent.Infrastructure;

public class ResourceRepository(ILogger<ResourceRepository> logger, MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor) : BaseRepository<Resource>(logger, client, optionsMonitor), IResourceRepository
{
    public Task<bool> CreateResources(List<Resource> resources, CancellationToken cancellation)
    {
        return CreateEntities(resources, cancellation);
    }

    public Task<List<Resource>> ListResources(CancellationToken cancellation)
    {
        return ListEntities(cancellation);
    }
}