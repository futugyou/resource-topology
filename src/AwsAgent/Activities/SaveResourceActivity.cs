
namespace AwsAgent.Activities;

public class SaveResourceActivity(IResourceRepository resourceRepository, IResourceRelationshipRepository resourceRelationshipRepository)
: WorkflowActivity<DifferentialResourcesRecord, bool>
{
    public override async Task<bool> RunAsync(WorkflowActivityContext context, DifferentialResourcesRecord input)
    {
        // TODO: dapr workflow does not currently support CancellationToken.
        // https://github.com/dapr/dotnet-sdk/issues/1225
        var cancellation = CancellationToken.None;
        var resTask = resourceRepository.BatchOperateAsync(input.InsertDatas, input.DeleteDatas.Select(p => p.Id).ToList(), input.UpdateDatas, cancellation);
        var shipTask = resourceRelationshipRepository.BatchOperateAsync(input.InsertShipDatas, input.DeleteShipDatas.Select(p => p.Id).ToList(), cancellation);
        await Task.WhenAll(resTask, shipTask);
        return true;
    }
}