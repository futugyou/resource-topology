namespace AwsAgent.Services;

public class DaprWorker : BackgroundService
{
    private readonly DaprWorkflowClient _daprWorkflowClient;

    public DaprWorker(DaprWorkflowClient daprWorkflowClient)
    {
        _daprWorkflowClient = daprWorkflowClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workflowId = Guid.NewGuid().ToString();
            await _daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(ResourceProcessorWorkflow), workflowId, "");
            await _daprWorkflowClient.WaitForWorkflowCompletionAsync(workflowId, cancellation: stoppingToken);
            var state = await _daprWorkflowClient.GetWorkflowStateAsync(workflowId);
            Console.WriteLine($"Workflow started with ID: {workflowId}, Workflow state: {state.RuntimeStatus}");

            await Task.Delay(5000, stoppingToken);
        }
    }
}
