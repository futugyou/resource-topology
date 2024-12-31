namespace KubeAgent.MonitorV2;

public interface IResourceMonitor
{
    Task StartMonitoringAsync(MonitoredResource resource, CancellationToken cancellation);
    Task StopMonitoringAsync(string resourceId);
}
