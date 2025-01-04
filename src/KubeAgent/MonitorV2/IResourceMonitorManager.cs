namespace KubeAgent.MonitorV2;

public interface IResourceMonitorManager
{
    Task MonitorResource(CancellationToken cancellation);
}