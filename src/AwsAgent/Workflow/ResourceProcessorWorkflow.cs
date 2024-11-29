namespace AwsAgent.Workflow;

public class ResourceProcessorWorkflow : Workflow<string, bool>
{
    public override async Task<bool> RunAsync(WorkflowContext context, string input)
    {
        context.SetCustomStatus("aws resource processing starting...");
        context.SetCustomStatus("1. get resources.");
        var dbDataTask = context.CallActivityAsync<(List<Resource>, List<ResourceRelationship>)>(nameof(GetDatabaseResourceActivity), input);
        var awsDataTask = context.CallActivityAsync<(List<Resource>, List<ResourceRelationship>)>(nameof(GetAwsResourceActivity), input);

        await Task.WhenAll(dbDataTask, awsDataTask);
        var dbData = dbDataTask.Result;
        var awsData = awsDataTask.Result;

        context.SetCustomStatus("2. calculate resources increment.");
        var incrementResource = new IncrementResource(dbData.Item1, dbData.Item2, awsData.Item1, awsData.Item2);
        var record = await context.CallActivityAsync<DifferentialResourcesRecord>(nameof(IncrementResourceActivity), incrementResource);
        if (!record.HasChange())
        {
            Console.WriteLine("no resources need to process.");
            context.SetCustomStatus("success, no resources need to process.");
            return true;
        }

        Console.WriteLine($@"{record.InsertDatas.Count} resources need to create, {record.DeleteDatas.Count} resources need to delete, {record.UpdateDatas.Count} resources need to update.
        {record.InsertShipDatas.Count} ships need to create, {record.DeleteShipDatas.Count} ships need to delete.");

        context.SetCustomStatus("3. save resources to DB.");
        await context.CallActivityAsync<bool>(nameof(SaveResourceActivity), record);

        context.SetCustomStatus("4. send resources processing event.");
        await context.CallActivityAsync<bool>(nameof(SendResourceProcessingEventActivity), record);

        context.SetCustomStatus("success, resources processing completed.");
        return true;
    }
}