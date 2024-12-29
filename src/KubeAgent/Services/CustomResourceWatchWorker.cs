
namespace KubeAgent.Services;

public class CustomResourceWatchWorker(ILogger<CustomResourceWatchWorker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, [FromKeyedServices("Custom")] IResourceProcessor processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.LogInformation("kube custom resource worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await processor.ProcessingData(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("kube custom resource worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("kube custom resource worker end at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.Complete(cancellationToken);
        await base.StopAsync(cancellationToken);
        logger.LogInformation("kube custom resource worker stop at: {time}", DateTimeOffset.Now);
    }
}
