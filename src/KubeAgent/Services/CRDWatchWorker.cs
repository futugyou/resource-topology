
namespace KubeAgent.Services;

public class CRDWatchWorker(ILogger<CRDWatchWorker> logger, IOptionsMonitor<AgentOptions> optionsMonitor, [FromKeyedServices("CRD")] IResourceProcessor processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.LogInformation("kube crd resource worker running at: {time}", DateTimeOffset.Now);

            try
            {
                await processor.ProcessingData(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("kube crd resource worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("kube crd resource worker end at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.Complete(cancellationToken);
        await base.StopAsync(cancellationToken);
        logger.LogInformation("kube crd resource worker stop at: {time}", DateTimeOffset.Now);
    }
}
