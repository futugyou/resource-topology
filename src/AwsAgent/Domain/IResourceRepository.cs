namespace AwsAgent.Domain;

public interface IResourceRepository
{
    Task<List<Resource>> ListResources(CancellationToken cancellation);
    Task<bool> CreateResources(List<Resource> resources, CancellationToken cancellation);
}