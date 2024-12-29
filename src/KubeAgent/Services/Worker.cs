
namespace KubeAgent.Services;

public class Worker(ILogger<Worker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.LogInformation("Kube agent worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await processor.ProcessingData(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Kube agent worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("Kube agent worker end at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.Complete(cancellationToken);
        await base.StopAsync(cancellationToken);
        logger.LogInformation("Kube agent worker stop at: {time}", DateTimeOffset.Now);
    }
}
