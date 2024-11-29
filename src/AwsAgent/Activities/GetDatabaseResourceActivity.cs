
namespace AwsAgent.Activities;

public class GetDatabaseResourceActivity(IResourceRepository resourceRepository, IResourceRelationshipRepository resourceRelationshipRepository)
: WorkflowActivity<string, ResourceAndShip>
{
    public override async Task<ResourceAndShip> RunAsync(WorkflowActivityContext context, string input)
    {
        // TODO: dapr workflow does not currently support CancellationToken.
        // https://github.com/dapr/dotnet-sdk/issues/1225
        var cancellation = CancellationToken.None;
        var dbResourcesTask = resourceRepository.ListResourcesAsync(cancellation);
        var dbResourceShiplTask = resourceRelationshipRepository.ListResourceRelationshipsAsync(cancellation);
        await Task.WhenAll(dbResourcesTask, dbResourceShiplTask);
        return new ResourceAndShip(dbResourcesTask.Result, dbResourceShiplTask.Result);
    }
}