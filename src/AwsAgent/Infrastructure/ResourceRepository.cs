namespace AwsAgent.Infrastructure;

public class ResourceRepository(MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor) : BaseRepository<Resource>(client, optionsMonitor), IResourceRepository
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