
namespace AwsAgent.Infrastructure;

public class ResourceRelationshipRepository(ILogger<ResourceRelationshipRepository> logger, MongoClient client, IOptionsMonitor<ServiceOption> optionsMonitor) : BaseRepository<ResourceRelationship>(logger, client, optionsMonitor), IResourceRelationshipRepository
{
    public Task<bool> BatchOperateAsync(List<ResourceRelationship> insertDatas, List<string> deleteIds, CancellationToken cancellation = default)
    {
        var deleteManyModel = new DeleteManyModel<ResourceRelationship>(Builders<ResourceRelationship>.Filter.In(e => e.Id, deleteIds ?? []));

        var insertOneModels = insertDatas?.Select(data => new InsertOneModel<ResourceRelationship>(data)) ?? [];

        var bulkWriteModels = new List<WriteModel<ResourceRelationship>> { deleteManyModel }
            .Concat(insertOneModels)
            .ToList();

        return BulkWriteAsync(bulkWriteModels, cancellation);
    }

    public Task<List<ResourceRelationship>> ListResourceRelationshipsAsync(CancellationToken cancellation)
    {
        return ListEntities(cancellation);
    }
}