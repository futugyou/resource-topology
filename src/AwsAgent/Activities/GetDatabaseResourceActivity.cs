
namespace AwsAgent.Activities;

public class GetDatabaseResourceActivity : WorkflowActivity<string, (List<Resource>, List<ResourceRelationship>)>
{
    public override Task<(List<Resource>, List<ResourceRelationship>)> RunAsync(WorkflowActivityContext context, string input)
    {
        return Task.FromResult<(List<Resource>, List<ResourceRelationship>)>(([], []));
    }
}