namespace AwsAgent.Workflow;

public class ResourceProcessorWorkflow : Workflow<string, bool>
{
    public override async Task<bool> RunAsync(WorkflowContext context, string input)
    {
        var dbDataTask = context.CallActivityAsync<(List<Resource>, List<ResourceRelationship>)>(nameof(GetDatabaseResourceActivity), input);
        var awsDataTask = context.CallActivityAsync<(List<Resource>, List<ResourceRelationship>)>(nameof(GetAwsResourceActivity), input);

        await Task.WhenAll(dbDataTask, awsDataTask);
        return true;
    }
}