
namespace KubeAgent.Services;

public interface IRestartResourceTracker
{
    Task AddRestartResource(RestartContext context, CancellationToken cancellation);

    Task<IEnumerable<RestartContext>> GetRestartResources(CancellationToken cancellation);
}

public class RestartContext
{
    public required string ResourceId { get; set; }
}