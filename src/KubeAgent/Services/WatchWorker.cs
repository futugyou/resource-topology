
using k8s.Models;

namespace KubeAgent.Services;

public class WatchWorker(ILogger<Worker> logger, IEnumerable<IResourceMonitor> monitors) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Kube agent watch worker running at: {time}", DateTimeOffset.Now);

        try
        {
            foreach (var monitor in monitors)
            {
                await monitor.MonitorResource(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Kube agent watch worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
        }

        logger.LogInformation("Kube agent watch woorker end at: {time}", DateTimeOffset.Now);
    }
}