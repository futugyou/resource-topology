namespace AwsAgent.Workflow;

public class ResourceProcessorWorkflow : Workflow<string, bool>
{
    public override async Task<bool> RunAsync(WorkflowContext context, string input)
    {
        // TODO: How to use ILog?
        var instanceId = context.InstanceId;
        Console.WriteLine($"{instanceId}: aws resource processing starting...");
        Console.WriteLine($"{instanceId}: 1. get resources");
        var dbDataTask = context.CallActivityAsync<ResourceAndShip>(nameof(GetDatabaseResourceActivity), input);
        var awsDataTask = context.CallActivityAsync<ResourceAndShip>(nameof(GetAwsResourceActivity), input);

        await Task.WhenAll(dbDataTask, awsDataTask);
        var dbData = dbDataTask.Result;
        var awsData = awsDataTask.Result;

        Console.WriteLine($"{instanceId}: 2. calculate resources increment.");
        var incrementResource = new IncrementResource(dbData.Resources, dbData.Ships, awsData.Resources, awsData.Ships);
        var record = await context.CallActivityAsync<DifferentialResourcesRecord>(nameof(IncrementResourceActivity), incrementResource);
        if (!record.HasChange())
        {
            Console.WriteLine($"{instanceId}: no resources need to process");
            context.SetCustomStatus("success, no resources need to process.");
            return true;
        }

        Console.WriteLine($@"{instanceId}: 2.1 {record.InsertDatas.Count} resources need to create, {record.DeleteDatas.Count} resources need to delete, {record.UpdateDatas.Count} resources need to update.
        {record.InsertShipDatas.Count} ships need to create, {record.DeleteShipDatas.Count} ships need to delete.");

        Console.WriteLine($"{instanceId}: 3. save resources to DB.");
        await context.CallActivityAsync<bool>(nameof(SaveResourceActivity), record);

        Console.WriteLine($"{instanceId}: 4. send resources processing event.");
        await context.CallActivityAsync<bool>(nameof(SendResourceProcessingEventActivity), record);

        context.SetCustomStatus("success, resources processing completed.");
        return true;
    }
}