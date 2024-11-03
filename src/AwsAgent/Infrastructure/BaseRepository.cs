namespace AwsAgent.Infrastructure;

public abstract class BaseRepository<TEntity> where TEntity : IEntity
{
    public ServiceOption ServiceOption { get; }
    public IMongoDatabase Database { get; }
    public MongoClient DatabaseClient { get; }
    public IMongoCollection<TEntity> Collection;
    private readonly ILogger<BaseRepository<TEntity>> _logger;

    public BaseRepository(ILogger<BaseRepository<TEntity>> logger, MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor)
    {
        _logger = logger;
        DatabaseClient = client;
        ServiceOption = optionsMonitor.CurrentValue;
        Database = client.GetDatabase(ServiceOption.DBName);
        Collection = Database.GetCollection<TEntity>(TEntity.GetCollectionName());
    }

    public async Task<List<TEntity>> ListEntities(CancellationToken cancellation)
    {
        var filter = Builders<TEntity>.Filter.Empty;
        return await Collection.Find(filter).ToListAsync(cancellation);
    }

    public async Task<bool> BulkWriteAsync(List<WriteModel<TEntity>> bulkWriteModels, CancellationToken cancellation = default)
    {
        // Creates an option object to bypass documentation validation on the documents
        // NEED admin role
        var op = new BulkWriteOptions { BypassDocumentValidation = true };
        var result = await Collection.BulkWriteAsync(bulkWriteModels, op, cancellation);
        _logger.LogInformation("{count} Inserted, {count} Deleted, {count} Modified in {name} ",
            result.InsertedCount, result.DeletedCount, result.ModifiedCount, TEntity.GetCollectionName());
        return true;
    }

}