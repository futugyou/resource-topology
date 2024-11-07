
namespace AwsAgent.Processor;

public class ResourceProcessor(ILogger<ResourceProcessor> logger, IResourceRepository resourceRepository,
    IResourceRelationshipRepository resourceRelationshipRepository, IResourceAdapterWrapper wrapper, DaprClient dapr) : AbstractResourceProcessor(logger)
{
    protected override Task<DifferentialResourcesRecord> DifferentialResourcesData(
        List<Resource> dbResources, List<ResourceRelationship> dbShips, List<Resource> awsResources, List<ResourceRelationship> awsShips, CancellationToken cancellation)
    {

        var insertDatas = GetExceptDatas(awsResources, dbResources);
        var deleteDatas = GetExceptDatas(dbResources, awsResources);
        var updateDatas = GetIntersectDatas(dbResources, awsResources);

        // ship do not need update, only insert and delete
        var insertShipDatas = GetExceptDatas(awsShips, dbShips);
        var deleteShipDatas = GetExceptDatas(dbShips, awsShips);

        return Task.FromResult(new DifferentialResourcesRecord(insertDatas, deleteDatas, updateDatas, insertShipDatas, deleteShipDatas));
    }

    protected override Task<(List<Resource>, List<ResourceRelationship>)> GetAdditionalResources(
        List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation)
    {
        return Task.FromResult((resources, ships));
    }

    protected override Task<(List<Resource>, List<ResourceRelationship>)> GetResourcesFromAWS(CancellationToken cancellation)
    {
        return wrapper.GetResourcAndRelationFromAWS(cancellation);
    }

    protected override async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcesFromDB(CancellationToken cancellation)
    {
        var dbResourcesTask = resourceRepository.ListResourcesAsync(cancellation);
        var dbResourceShiplTask = resourceRelationshipRepository.ListResourceRelationshipsAsync(cancellation);
        await Task.WhenAll(dbResourcesTask, dbResourceShiplTask);
        return (dbResourcesTask.Result, dbResourceShiplTask.Result);
    }

    protected override Task<(List<Resource>, List<ResourceRelationship>)> MergeResources(
        List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation)
    {
        return Task.FromResult((resources, ships));
    }

    protected override Task SaveDifferentialDatas(DifferentialResourcesRecord record, CancellationToken cancellation)
    {
        var resTask = resourceRepository.BatchOperateAsync(record.InsertDatas, record.DeleteDatas.Select(p => p.Id).ToList(), record.UpdateDatas, cancellation);
        var shipTask = resourceRelationshipRepository.BatchOperateAsync(record.InsertShipDatas, record.DeleteShipDatas.Select(p => p.Id).ToList(), cancellation);
        return Task.WhenAll(resTask, shipTask);
    }

    protected override Task SendResourceProcessingEvent(DifferentialResourcesRecord record, CancellationToken cancellation)
    {
        var processorEvent = ConvertResourceToEvent(record.InsertDatas, record.DeleteDatas, record.UpdateDatas, record.InsertShipDatas, record.DeleteShipDatas);
        return dapr.PublishEventAsync("resource-agent", "resources", processorEvent, cancellation);
    }

    private static List<Entity> GetExceptDatas<Entity>(List<Entity> first, List<Entity> second) where Entity : IEntity
    {
        return first.ExceptBy(second.Select(d => d.Id), d => d.Id).ToList();
    }

    private static List<Resource> GetIntersectDatas(List<Resource> first, List<Resource> second)
    {
        return (from a in first
                join b in second on a.Id equals b.Id
                where a.ResourceHash != b.ResourceHash
                select b).ToList();
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