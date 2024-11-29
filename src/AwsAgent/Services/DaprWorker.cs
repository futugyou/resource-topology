namespace AwsAgent.Services;

public class DaprWorker(ILogger<Worker> logger, IServiceProvider servicerovider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = servicerovider.CreateAsyncScope();
            var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ServiceOption>>();
            var _daprWorkflowClient = scope.ServiceProvider.GetRequiredService<DaprWorkflowClient>();
            var serviceOption = optionsMonitor.CurrentValue;

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

            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }
}
