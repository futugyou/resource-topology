namespace AwsAgent.Infrastructure;

public class ResourceRelationshipRepository : IResourceRelationshipRepository
{
    private readonly MongoClient _client;
    private readonly DBOption _option;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<ResourceRelationship> _collection;

    public ResourceRelationshipRepository(MongoClient client, IOptionsMonitor<DBOption> optionsMonitor)
    {
        _client = client;
        _option = optionsMonitor.CurrentValue;
        _database = client.GetDatabase(_option.DBName);
        _collection = _database.GetCollection<ResourceRelationship>(ResourceRelationship.GetCollectionName());
    }

    public async Task<bool> CreateResourceRelationships(List<ResourceRelationship> relationships, CancellationToken cancellation)
    {
        // Creates an option object to bypass documentation validation on the documents
        // NEED admin role
        var options = new InsertManyOptions() { BypassDocumentValidation = true };
        await _collection.InsertManyAsync(relationships, options, cancellation);
        return true;
    }

    public async Task<List<ResourceRelationship>> ListResourceRelationships(CancellationToken cancellation)
    {
        var filter = Builders<ResourceRelationship>.Filter.Empty;
        return await _collection.Find(filter).ToListAsync(cancellation);
    }

}