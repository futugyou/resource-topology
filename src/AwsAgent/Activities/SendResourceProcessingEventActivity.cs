
namespace AwsAgent.Activities;

public class SendResourceProcessingEventActivity(DaprClient dapr, IMapper mapper, IOptionsMonitor<ServiceOption> optionsMonitor) : WorkflowActivity<DifferentialResourcesRecord, bool>
{
    public override async Task<bool> RunAsync(WorkflowActivityContext context, DifferentialResourcesRecord input)
    {
        // TODO: dapr workflow does not currently support CancellationToken.
        // https://github.com/dapr/dotnet-sdk/issues/1225
        var cancellation = CancellationToken.None;
        var processorEvent = mapper.Map<ResourceContracts.ResourceProcessorEvent>(input);

        var serviceOption = optionsMonitor.CurrentValue!;
        if (serviceOption.DaprStateOutboxSupported)
        {
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
        }
        else
        {
            await dapr.PublishEventAsync("resource-agent", "resources", processorEvent, cancellation);
        }

        return true;
    }
}