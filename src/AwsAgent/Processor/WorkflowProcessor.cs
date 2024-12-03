
namespace AwsAgent.Processor;

public class WorkflowProcessor(DaprWorkflowClient daprWorkflowClient, ILogger<WorkflowProcessor> logger) : IResourceProcessor
{
    public async Task ProcessingData(CancellationToken cancellation)
    {
        var workflowId = $"aws-resource-process-{Guid.NewGuid()}";
        try
        {
            var schedule = await daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(ResourceProcessorWorkflow), workflowId, "");
            logger.LogInformation("ScheduleNewWorkflowAsync result: {schedule}", schedule);
            var state = await daprWorkflowClient.WaitForWorkflowCompletionAsync(workflowId, cancellation: cancellation);
            logger.LogInformation("Workflow started with ID: {workflowId}, Workflow state: {state.RuntimeStatus}", workflowId, state.ReadCustomStatusAs<string>());
        }
        catch (Exception ex)
        {
            try
            {
                await daprWorkflowClient.PurgeInstanceAsync(workflowId, cancellation);
            }
            catch (Exception)
            {
            }
            logger.LogError("DaprWorker running at: {time}, and get an error: {error}", DateTimeOffset.Now, ex.Message);
        }
    }
}