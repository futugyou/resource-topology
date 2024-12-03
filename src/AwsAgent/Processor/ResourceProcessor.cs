
namespace AwsAgent.Processor;

public class ResourceProcessor(ILogger<ResourceProcessor> logger, IResourceRepository resourceRepository,
    IResourceRelationshipRepository resourceRelationshipRepository, IResourceAdapterWrapper wrapper, DaprClient dapr,
    IMapper mapper, IOptionsMonitor<ServiceOption> optionsMonitor) : AbstractResourceProcessor(logger)
{
    protected override Task<DifferentialResourcesRecord> DifferentialResourcesData(
        List<Resource> dbResources, List<ResourceRelationship> dbShips, List<Resource> awsResources, List<ResourceRelationship> awsShips, CancellationToken cancellation)
    {

        var insertDatas = Util.GetExceptDatas(awsResources, dbResources);
        var deleteDatas = Util.GetExceptDatas(dbResources, awsResources);
        var updateDatas = Util.GetIntersectDatas(dbResources, awsResources);

        // ship do not need update, only insert and delete
        var insertShipDatas = Util.GetExceptDatas(awsShips, dbShips);
        var deleteShipDatas = Util.GetExceptDatas(dbShips, awsShips);

        return Task.FromResult(new DifferentialResourcesRecord(insertDatas, deleteDatas, updateDatas, insertShipDatas, deleteShipDatas));
    }

    protected override Task<ResourceAndShip> GetResourcesFromAWS(CancellationToken cancellation)
    {
        return wrapper.GetResourcAndRelationFromAWS(cancellation);
    }

    protected override async Task<ResourceAndShip> GetResourcesFromDB(CancellationToken cancellation)
    {
        var dbResourcesTask = resourceRepository.ListResourcesAsync(cancellation);
        var dbResourceShiplTask = resourceRelationshipRepository.ListResourceRelationshipsAsync(cancellation);
        await Task.WhenAll(dbResourcesTask, dbResourceShiplTask);
        return new ResourceAndShip(dbResourcesTask.Result, dbResourceShiplTask.Result);
    }

    protected override Task SaveDifferentialDatas(DifferentialResourcesRecord record, CancellationToken cancellation)
    {
        var resTask = resourceRepository.BatchOperateAsync(record.InsertDatas, record.DeleteDatas.Select(p => p.Id).ToList(), record.UpdateDatas, cancellation);
        var shipTask = resourceRelationshipRepository.BatchOperateAsync(record.InsertShipDatas, record.DeleteShipDatas.Select(p => p.Id).ToList(), cancellation);
        return Task.WhenAll(resTask, shipTask);
    }

    protected override Task SendResourceProcessingEvent(DifferentialResourcesRecord record, CancellationToken cancellation)
    {
        var processorEvent = mapper.Map<ResourceContracts.ResourceProcessorEvent>(record);
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

            return dapr.ExecuteStateTransactionAsync("aws-agent-state", upsert, cancellationToken: cancellation);
        }

        return dapr.PublishEventAsync("resource-agent", "resources", processorEvent, cancellation);
    }
}