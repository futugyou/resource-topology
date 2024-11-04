using ProcessorEvent = ResourceContracts.ResourceProcessorEvent;

namespace AwsAgent.Services;

public class ResourceProcessor(ILogger<ResourceProcessor> logger, IOptionsMonitor<ServiceOption> optionsMonitor,
    IResourceRepository resourceRepository, IResourceRelationshipRepository resourceRelationshipRepository, IResourceAdapterWrapper wrapper, DaprClient dapr) : IResourceProcessor
{
    public async Task ProcessingData(CancellationToken cancellation)
    {
        var option = optionsMonitor.CurrentValue;
        // 1. get data from db 
        var dbResourcesTask = resourceRepository.ListResourcesAsync(cancellation);
        var dbResourceShiplTask = resourceRelationshipRepository.ListResourceRelationshipsAsync(cancellation);

        // 2. get data from aws 
        Task<(List<Resource> awsResources, List<ResourceRelationship> awsResourceShips)> awsTask = wrapper.GetResourcAndRelationFromAWS(cancellation);

        // 3. merge data to db
        await Task.WhenAll(dbResourcesTask, dbResourceShiplTask, awsTask);
        var dbResources = dbResourcesTask.Result;
        var dbResourceShips = dbResourceShiplTask.Result;
        var awsResources = awsTask.Result.awsResources;
        var awsResourceShips = awsTask.Result.awsResourceShips;

        var insertDatas = GetExceptDatas(awsResources, dbResources);
        var deleteDatas = GetExceptDatas(dbResources, awsResources);
        var updateDatas = GetIntersectDatas(dbResources, awsResources);

        // ship do not need update, only insert and delete
        var insertShipDatas = GetExceptDatas(awsResourceShips, dbResourceShips);
        var deleteShipDatas = GetExceptDatas(dbResourceShips, awsResourceShips);

        logger.LogInformation("{count} resources need to create, {count} resources need to delete, {count} resources need to update", insertDatas.Count, deleteDatas.Count, updateDatas.Count);
        logger.LogInformation("{count} ships need to create, {count} ships need to delete,", insertShipDatas.Count, deleteShipDatas.Count);

        if (insertDatas.Count == 0 && deleteDatas.Count == 0 && updateDatas.Count == 0 && insertShipDatas.Count == 0 && deleteShipDatas.Count == 0)
        {
            return;
        }

        // TODO: Is it necessary to open a transaction for such a simple operation?
        await resourceRepository.BatchOperateAsync(insertDatas, deleteDatas.Select(p => p.Id).ToList(), updateDatas, cancellation);
        await resourceRelationshipRepository.BatchOperateAsync(insertShipDatas, deleteShipDatas.Select(p => p.Id).ToList(), cancellation);

        var processorEvent = ConvertResourceToEvent(insertDatas, deleteDatas, updateDatas, insertShipDatas, deleteShipDatas);
        await dapr.PublishEventAsync("resource-agent", "resources", processorEvent, cancellation);
    }

    private static ProcessorEvent ConvertResourceToEvent(List<Resource> insertDatas, List<Resource> deleteDatas, List<Resource> updateDatas, List<ResourceRelationship> insertShipDatas, List<ResourceRelationship> deleteShipDatas)
    {
        return new ProcessorEvent
        {
            InsertResources = insertDatas.Select(ConvertResource).ToList(),
            DeleteResources = deleteDatas.Select(p => p.Id).ToList(),
            UpdateResources = updateDatas.Select(ConvertResource).ToList(),
            InsertShips = insertShipDatas.Select(ConvertRelationship).ToList(),
            DeleteShips = deleteShipDatas.Select(p => p.Id).ToList(),
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
            ResourceHash = res.ConfigurationItemCaptureTime.ToString(),
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

    private static List<Entity> GetExceptDatas<Entity>(List<Entity> first, List<Entity> second) where Entity : IEntity
    {
        return first.ExceptBy(second.Select(d => d.Id), d => d.Id).ToList();
    }

    private static List<Resource> GetIntersectDatas(List<Resource> first, List<Resource> second)
    {
        return (from a in first
                join b in second on a.Id equals b.Id
                where a.ConfigurationItemCaptureTime != b.ConfigurationItemCaptureTime
                select b).ToList();
    }
}
