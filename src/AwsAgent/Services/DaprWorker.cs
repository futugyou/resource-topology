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
            //TODO: create meaning Id
            var workflowId = Guid.NewGuid().ToString();
            try
            {
                await _daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(ResourceProcessorWorkflow), workflowId, "");
                await _daprWorkflowClient.WaitForWorkflowCompletionAsync(workflowId, cancellation: stoppingToken);
                var state = await _daprWorkflowClient.GetWorkflowStateAsync(workflowId);
                Console.WriteLine($"Workflow started with ID: {workflowId}, Workflow state: {state.RuntimeStatus}");
            }
            catch (Exception ex)
            {
                logger.LogError("DaprWorker running at: {time}, and get an error: {error}", DateTimeOffset.Now, ex.Message);
            }

            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }
}
