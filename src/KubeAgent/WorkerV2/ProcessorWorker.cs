
using KubeAgent.ProcessorV2;

namespace KubeAgent.WorkerV2;

public class ProcessorWorker(ILogger<ProcessorWorker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, [FromKeyedServices("General")] IDataProcessor<Resource> processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.ProcessorWorkerRunning(DateTimeOffset.Now);

            try
            {
                await processor.ProcessingData(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.ProcessorWorkerError(DateTimeOffset.Now, ex);
            }

            logger.ProcessorWorkerEnd(DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.Complete(cancellationToken);
        await base.StopAsync(cancellationToken);
        logger.ProcessorWorkerStop(DateTimeOffset.Now);
    }
}
