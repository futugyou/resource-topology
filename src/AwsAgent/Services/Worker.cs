namespace AwsAgent.Services;

public class Worker(ILogger<Worker> logger, IServiceProvider servicerovider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = servicerovider.CreateAsyncScope();
            var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ServiceOption>>();
            var serviceOption = optionsMonitor.CurrentValue!;
            var processorKey = serviceOption.DaprWorkflowSupported ? "Workflow" : "Normal";
            logger.LogInformation("Worker running at: {time}, processor is {key}", DateTimeOffset.Now, processorKey);

            try
            {
                var processor = scope.ServiceProvider.GetRequiredKeyedService<IResourceProcessor>(processorKey);
                await processor.ProcessingData(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("Worker end at: {time}", DateTimeOffset.Now);

            if (serviceOption.RunSingle)
            {
                // var host = servicerovider.GetRequiredService<IHost>();
                // await host.StopAsync(stoppingToken);
                hostApplicationLifetime.StopApplication();
                break;
            }

            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }
}
