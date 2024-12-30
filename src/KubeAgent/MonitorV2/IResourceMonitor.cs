namespace KubeAgent.MonitorV2;

public interface IResourceMonitor
{
    Task StartMonitoringAsync(MonitoredResource resource);
    Task StopMonitoringAsync(string resourceId);
}
