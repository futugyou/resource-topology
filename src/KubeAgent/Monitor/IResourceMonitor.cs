namespace KubeAgent.Monitor;

public interface IResourceMonitor
{
    Task MonitorResource(CancellationToken cancellation);
}