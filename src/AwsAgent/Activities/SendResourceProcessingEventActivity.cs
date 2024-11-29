
namespace AwsAgent.Activities;

public class SendResourceProcessingEventActivity(DaprClient dapr, IMapper mapper) : WorkflowActivity<DifferentialResourcesRecord, bool>
{
    public override async Task<bool> RunAsync(WorkflowActivityContext context, DifferentialResourcesRecord input)
    {
        // TODO: dapr workflow does not currently support CancellationToken.
        // https://github.com/dapr/dotnet-sdk/issues/1225
        var cancellation = CancellationToken.None;
        var processorEvent = mapper.Map<ResourceContracts.ResourceProcessorEvent>(input);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(processorEvent);
        var metadata = new Dictionary<string, string> {
            {"datacontenttype","application/json"},
            {"contentType","application/json"},
            {"ttlInSeconds","86400"},
        };
        var upsert = new List<StateTransactionRequest>()
        {
            new(Guid.NewGuid().ToString(), bytes, StateOperationType.Upsert, metadata:metadata)
        };

        await dapr.ExecuteStateTransactionAsync("aws-agent-state", upsert, cancellationToken: cancellation);
        return true;
    }
}