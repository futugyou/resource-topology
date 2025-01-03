namespace KubeAgent.Discovery;

public interface IAdditionResourceProvider
{
    Task AddAdditionResource(MonitoredResource resource, CancellationToken cancellation);
}
