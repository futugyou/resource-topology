namespace AwsAgent.Domain;

public interface IResourceRelationshipRepository
{
    Task<List<ResourceRelationship>> ListResourceRelationships(CancellationToken cancellation);
    Task<bool> CreateResourceRelationships(List<ResourceRelationship> relationships, CancellationToken cancellation);
    Task<bool> BulkWriteAsync(List<ResourceRelationship> insertDatas, List<string> deleteIds, CancellationToken cancellation = default);
}