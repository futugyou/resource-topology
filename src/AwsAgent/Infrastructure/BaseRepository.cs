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

    public async Task<bool> BulkWriteAsync(List<TEntity> insertDatas, List<string> deleteIds, List<TEntity> updateDatas, CancellationToken cancellation = default)
    {
        var deleteManyModel = new DeleteManyModel<TEntity>(Builders<TEntity>.Filter.In(e => e.Id, deleteIds ?? []));

        var insertOneModels = insertDatas?.Select(data => new InsertOneModel<TEntity>(data)) ?? [];

        var replaceOneModels = updateDatas?.Select(data => new ReplaceOneModel<TEntity>(Builders<TEntity>.Filter.Eq(e => e.Id, data.Id), data)) ?? [];

        var bulkWriteModels = new List<WriteModel<TEntity>> { deleteManyModel }
            .Concat(insertOneModels)
            .Concat(replaceOneModels)
            .ToList();

        var result = await Collection.BulkWriteAsync(bulkWriteModels, null, cancellation);
        _logger.LogInformation("{count} Inserted, {count} Deleted, {count} Modified in {name} ",
            result.InsertedCount, result.DeletedCount, result.ModifiedCount, TEntity.GetCollectionName());
        return true;
    }

}