
namespace KubeAgent.MonitorV2;

public class GeneralMonitor : IResourceMonitor
{
    public Task StartMonitoringAsync(MonitoredResource resource)
    {
        return Task.CompletedTask;
    }

    public Task StopMonitoringAsync(string resourceId)
    {
        return Task.CompletedTask;
    }
}
