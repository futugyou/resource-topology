namespace AwsAgent.Services;

public class Worker(ILogger<Worker> logger, IServiceProvider servicerovider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            var scope = servicerovider.CreateAsyncScope();
            var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ServiceOption>>();
            var serviceOption = optionsMonitor.CurrentValue!;

            if (serviceOption.DaprWorkflowSupported)
            {
                await DaprWorkflowProcessor(scope, stoppingToken);
            }
            else
            {
                await NormalProcessor(scope, stoppingToken);
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

    private async Task DaprWorkflowProcessor(IServiceScope scope, CancellationToken stoppingToken)
    {
        var _daprWorkflowClient = scope.ServiceProvider.GetRequiredService<DaprWorkflowClient>();

        var workflowId = $"aws-resource-process-{Guid.NewGuid()}";
        try
        {
            var schedule = await _daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(ResourceProcessorWorkflow), workflowId, "");
            logger.LogInformation("ScheduleNewWorkflowAsync result: {schedule}", schedule);
            var state = await _daprWorkflowClient.WaitForWorkflowCompletionAsync(workflowId, cancellation: stoppingToken);
            logger.LogInformation("Workflow started with ID: {workflowId}, Workflow state: {state.RuntimeStatus}", workflowId, state.ReadCustomStatusAs<string>());
        }
        catch (Exception ex)
        {
            try
            {
                await _daprWorkflowClient.PurgeInstanceAsync(workflowId, stoppingToken);
            }
            catch (Exception)
            {
            }
            logger.LogError("DaprWorker running at: {time}, and get an error: {error}", DateTimeOffset.Now, ex.Message);
        }
    }

    private async Task NormalProcessor(IServiceScope scope, CancellationToken stoppingToken)
    {
        try
        {
            var processor = scope.ServiceProvider.GetRequiredService<IResourceProcessor>();
            await processor.ProcessingData(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError("Worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, ex.Message);
        }
    }
}
