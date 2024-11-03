namespace AwsAgent.Domain;

public interface IResourceRelationshipRepository
{
    Task<List<ResourceRelationship>> ListResourceRelationshipsAsync(CancellationToken cancellation); 
    Task<bool> BatchOperateAsync(List<ResourceRelationship> insertDatas, List<string> deleteIds, CancellationToken cancellation = default);
}