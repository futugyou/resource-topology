
namespace KubeAgent.Worker;

public class MonitorWorker(ILogger<MonitorWorker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, IResourceMonitorManager processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.MonitorWorkerRunning(DateTimeOffset.Now);

            try
            {
                await processor.MonitorResource(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.MonitorWorkerError(DateTimeOffset.Now, ex);
            }

            logger.MonitorWorkerEnd(DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        logger.MonitorWorkerStop(DateTimeOffset.Now);
    }
}
