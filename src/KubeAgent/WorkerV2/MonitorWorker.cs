
using KubeAgent.MonitorV2;

namespace KubeAgent.WorkerV2;

public class MonitorWorker(ILogger<MonitorWorker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, IResourceMonitorManager processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.LogInformation("resource worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await processor.MonitorResource(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("resource worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("resource worker end at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        logger.LogInformation("resource worker stop at: {time}", DateTimeOffset.Now);
    }
}
