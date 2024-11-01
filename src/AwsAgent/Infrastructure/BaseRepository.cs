namespace AwsAgent.Infrastructure;

public abstract class BaseRepository<TEntity> where TEntity : IEntity
{
    public ServiceOption ServiceOption { get; }
    public IMongoDatabase Database { get; }
    public IMongoCollection<TEntity> Collection;

    public BaseRepository(MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor)
    {
        ServiceOption = optionsMonitor.CurrentValue;
        Database = client.GetDatabase(ServiceOption.DBName);
        Collection = Database.GetCollection<TEntity>(TEntity.GetCollectionName());
    }

    public async Task<bool> CreateEntities(List<TEntity> resources, CancellationToken cancellation)
    {
        // Creates an option object to bypass documentation validation on the documents
        // NEED admin role
        var options = new InsertManyOptions() { BypassDocumentValidation = true };
        await Collection.InsertManyAsync(resources, options, cancellation);
        return true;
    }

    public async Task<List<TEntity>> ListEntities(CancellationToken cancellation)
    {
        var filter = Builders<TEntity>.Filter.Empty;
        return await Collection.Find(filter).ToListAsync(cancellation);
    }
}