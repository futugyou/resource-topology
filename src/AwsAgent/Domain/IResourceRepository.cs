namespace Domain;

public interface IResourceRepository
{
    Task<List<Resource>> ListResources(CancellationToken cancellation);
}