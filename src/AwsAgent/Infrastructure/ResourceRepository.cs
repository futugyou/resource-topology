namespace AwsAgent.Infrastructure;

public class ResourceRepository(ILogger<ResourceRepository> logger, MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor) : BaseRepository<Resource>(logger, client, optionsMonitor), IResourceRepository
{
    public Task<List<Resource>> ListResourcesAsync(CancellationToken cancellation)
    {
        return ListEntities(cancellation);
    }

    public Task<bool> BatchOperateAsync(List<Resource> insertDatas, List<string> deleteIds, List<Resource> updateDatas, CancellationToken cancellation = default)
    {
        var deleteManyModel = new DeleteManyModel<Resource>(Builders<Resource>.Filter.In(e => e.Id, deleteIds ?? []));

        var insertOneModels = insertDatas?.Select(data => new InsertOneModel<Resource>(data)) ?? [];

        var replaceOneModels = updateDatas?.Select(data => new ReplaceOneModel<Resource>(Builders<Resource>.Filter.Eq(e => e.Id, data.Id), data)) ?? [];

        var bulkWriteModels = new List<WriteModel<Resource>> { deleteManyModel }
            .Concat(insertOneModels)
            .Concat(replaceOneModels)
            .ToList();
        return BulkWriteAsync(bulkWriteModels, cancellation);
    }
}