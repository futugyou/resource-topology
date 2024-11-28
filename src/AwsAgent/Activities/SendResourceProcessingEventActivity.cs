
namespace AwsAgent.Activities;

public class SendResourceProcessingEventActivity(DaprClient dapr) : WorkflowActivity<DifferentialResourcesRecord, bool>
{
    public override async Task<bool> RunAsync(WorkflowActivityContext context, DifferentialResourcesRecord input)
    {
        // TODO: dapr workflow does not currently support CancellationToken.
        // https://github.com/dapr/dotnet-sdk/issues/1225
        var cancellation = CancellationToken.None;
        // TODO: need automapper
        var processorEvent = ConvertResourceToEvent(input.InsertDatas, input.DeleteDatas, input.UpdateDatas, input.InsertShipDatas, input.DeleteShipDatas);
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

    private static ResourceContracts.ResourceProcessorEvent ConvertResourceToEvent(
        List<Resource> insertDatas, List<Resource> deleteDatas, List<Resource> updateDatas,
        List<ResourceRelationship> insertShipDatas, List<ResourceRelationship> deleteShipDatas)
    {
        return new ResourceContracts.ResourceProcessorEvent
        {
            InsertResources = insertDatas.Select(ConvertResource).ToList(),
            DeleteResources = deleteDatas.Select(p => p.Id).ToList(),
            UpdateResources = updateDatas.Select(ConvertResource).ToList(),
            InsertShips = insertShipDatas.Select(ConvertRelationship).ToList(),
            DeleteShips = deleteShipDatas.Select(p => p.Id).ToList(),
            Provider = "Aws",
        };
    }

     private static ResourceContracts.ResourceRelationship ConvertRelationship(ResourceRelationship ship)
    {
        return new()
        {
            Id = ship.Id,
            Relation = ship.Label,
            SourceId = ship.SourceId,
            TargetId = ship.TargetId,
        };
    }

    private static ResourceContracts.Resource ConvertResource(Resource res)
    {
        return new()
        {
            Id = res.Id,
            ResourceHash = res.ResourceHash,
            ResourceCreationTime = res.ResourceCreationTime,
            Configuration = res.Configuration,
            AvailabilityZone = res.AvailabilityZone,
            Region = res.AwsRegion,
            AccountID = res.AccountID,
            ResourceType = res.ResourceType,
            ResourceName = res.ResourceName,
            ResourceID = res.ResourceID,
            ResourceUrl = res.ResourceUrl,
            Tags = res.Tags.ToDictionary(tag => tag.Key, tag => tag.Value),
        };
    }
}