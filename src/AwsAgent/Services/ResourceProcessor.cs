
namespace AwsAgent.Services;

public class ResourceProcessor(ILogger<ResourceProcessor> logger, IOptionsMonitor<ServiceOption> optionsMonitor,
    IResourceRepository resourceRepository, IResourceRelationshipRepository resourceRelationshipRepository, IAMResourceAdapter iamAdapter, DaprClient dapr) : IResourceProcessor
{
    public async Task ProcessingData(CancellationToken cancellation)
    {
        var option = optionsMonitor.CurrentValue;
        // 1. get data from db 
        var dbResourcesTask = resourceRepository.ListResources(cancellation);
        var dbResourceShiplTask = resourceRelationshipRepository.ListResourceRelationships(cancellation);

        // 2. get data from aws 
        Task<(List<Resource> awsResources, List<ResourceRelationship> awsResourceShips)> awsTask;
        if (option.AwsconfigSupported)
        {
            awsTask = iamAdapter.ConvertConfigToResource(cancellation);
        }
        else
        {
            awsTask = iamAdapter.ConvertIAMToResource(cancellation);
        }

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

        await resourceRepository.BulkWriteAsync(insertDatas, deleteDatas.Select(p => p.Id).ToList(), updateDatas, cancellation);
        await resourceRelationshipRepository.BulkWriteAsync(insertShipDatas, deleteShipDatas.Select(p => p.Id).ToList(), cancellation);

        await dapr.PublishEventAsync("resource-agent", "resources",
        new ResourceProcessorEvent(insertDatas, deleteDatas.Select(p => p.Id).ToList(), updateDatas, insertShipDatas, deleteShipDatas.Select(p => p.Id).ToList()), cancellation);
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
