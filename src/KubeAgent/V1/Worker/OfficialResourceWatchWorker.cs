using IResourceMonitor = KubeAgent.V1.Monitor.IResourceMonitor;

namespace KubeAgent.V1.Worker;

public class OfficialResourceWatchWorker(ILogger<OfficialResourceWatchWorker> logger, IEnumerable<IResourceMonitor> monitors) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("kube official resources worker running at: {time}", DateTimeOffset.Now);

        try
        {
            foreach (var monitor in monitors)
            {
                await monitor.MonitorResource(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("kube official resources worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
        }

        logger.LogInformation("kube official resources worker end at: {time}", DateTimeOffset.Now);
    }
}