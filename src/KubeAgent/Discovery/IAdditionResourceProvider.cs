namespace KubeAgent.Discovery;

/// <summary>
/// Provides a way to dynamically add resource monitoring lists, mainly used for CustomResource(CR).
/// </summary>
public interface IAdditionResourceProvider
{
    Task AddAdditionResource(MonitoredResource resource, CancellationToken cancellation);
    Task RemoveAdditionResource(string resourceId, CancellationToken cancellation);
}
