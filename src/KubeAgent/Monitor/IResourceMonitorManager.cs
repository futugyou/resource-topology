namespace KubeAgent.Monitor;

public interface IResourceMonitorManager
{
    Task MonitorResource(CancellationToken cancellation);
}