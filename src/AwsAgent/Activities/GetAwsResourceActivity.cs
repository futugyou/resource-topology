
namespace AwsAgent.Activities;

public class GetAwsResourceActivity(IResourceAdapterWrapper wrapper) : WorkflowActivity<string, ResourceAndShip>
{
    public override Task<ResourceAndShip> RunAsync(WorkflowActivityContext context, string input)
    {
        // TODO: dapr workflow does not currently support CancellationToken.
        // https://github.com/dapr/dotnet-sdk/issues/1225
        var cancellation = CancellationToken.None;
        return wrapper.GetResourcAndRelationFromAWS(cancellation);
    }
}