namespace AwsAgent.Infrastructure;

public class ResourceRepository : IResourceRepository
{
    private readonly MongoClient _client;
    private readonly ServiceOption _option;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Resource> _collection;

    public ResourceRepository(MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor)
    {
        _client = client;
        _option = optionsMonitor.CurrentValue;
        _database = client.GetDatabase(_option.DBName);
        _collection = _database.GetCollection<Resource>(Resource.GetCollectionName());
    }

    public async Task<bool> CreateResources(List<Resource> resources, CancellationToken cancellation)
    {
        // Creates an option object to bypass documentation validation on the documents
        // NEED admin role
        var options = new InsertManyOptions() { BypassDocumentValidation = true };
        await _collection.InsertManyAsync(resources, options, cancellation);
        return true;
    }

    public async Task<List<Resource>> ListResources(CancellationToken cancellation)
    {
        var filter = Builders<Resource>.Filter.Empty;
        return await _collection.Find(filter).ToListAsync(cancellation);
    }
}