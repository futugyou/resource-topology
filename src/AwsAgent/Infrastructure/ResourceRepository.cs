namespace AwsAgent.Infrastructure;

public class ResourceRepository : IResourceRepository
{
    private readonly MongoClient _client;
    private readonly DBOption _option;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Resource> _collection;

    public ResourceRepository(MongoClient client, IOptionsMonitor<DBOption> optionsMonitor)
    {
        _client = client;
        _option = optionsMonitor.CurrentValue;
        _database = client.GetDatabase(_option.DBName);
        _collection = _database.GetCollection<Resource>(_option.ResourceCollectionName);
    }

    public async Task<bool> CreateResources(List<Resource> Resources, CancellationToken cancellation)
    {
        // Creates an option object to bypass documentation validation on the documents
        var options = new InsertManyOptions() { BypassDocumentValidation = true };
        await _collection.InsertManyAsync(Resources, options, cancellation);
        return true;
    }

    public async Task<List<Resource>> ListResources(CancellationToken cancellation)
    {
        var filter = Builders<Resource>.Filter.Empty;
        return await _collection.Find(filter).ToListAsync(cancellation);
    }
}