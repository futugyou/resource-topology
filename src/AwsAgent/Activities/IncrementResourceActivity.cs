
namespace AwsAgent.Activities;
 
public class IncrementResourceActivity() : WorkflowActivity<IncrementResource, DifferentialResourcesRecord>
{
    public override Task<DifferentialResourcesRecord> RunAsync(WorkflowActivityContext context, IncrementResource input)
    {
        var insertDatas = Util.GetExceptDatas(input.AWSResources, input.DBResources);
        var deleteDatas = Util.GetExceptDatas(input.DBResources, input.AWSResources);
        var updateDatas = Util.GetIntersectDatas(input.DBResources, input.AWSResources);

        // ship do not need update, only insert and delete
        var insertShipDatas = Util.GetExceptDatas(input.AWSShips, input.DBShips);
        var deleteShipDatas = Util.GetExceptDatas(input.DBShips, input.AWSShips);

        return Task.FromResult(new DifferentialResourcesRecord(insertDatas, deleteDatas, updateDatas, insertShipDatas, deleteShipDatas));
    }
    
}