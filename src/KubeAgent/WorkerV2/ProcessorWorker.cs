
using KubeAgent.ProcessorV2;

namespace KubeAgent.WorkerV2;

public class ProcessorWorker(ILogger<ProcessorWorker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, [FromKeyedServices("General")] IDataProcessor<Resource> processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.LogInformation("kube processor worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await processor.ProcessingData(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("kube processor worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("kube processor worker end at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.Complete(cancellationToken);
        await base.StopAsync(cancellationToken);
        logger.LogInformation("kube processor worker stop at: {time}", DateTimeOffset.Now);
    }
}
