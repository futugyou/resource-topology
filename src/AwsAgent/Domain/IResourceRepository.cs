namespace AwsAgent.Domain;

public interface IResourceRepository
{
    Task<List<Resource>> ListResourcesAsync(CancellationToken cancellation);
    Task<bool> BatchOperateAsync(List<Resource> insertDatas, List<string> deleteIds, List<Resource> updateDatas, CancellationToken cancellation = default);
}