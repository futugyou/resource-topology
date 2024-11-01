namespace AwsAgent.Services;

public class ResourceProcessor(ILogger<ResourceProcessor> logger, IOptionsMonitor<ServiceOption> optionsMonitor,
    IResourceRepository resourceRepository, IAMResourceAdapter iamAdapter, DaprClient dapr) : IResourceProcessor
{
    public async Task ProcessingData(CancellationToken cancellation)
    {
        var option = optionsMonitor.CurrentValue;
        // 1. get data from db 
        var dbResourcesTask = resourceRepository.ListResources(cancellation);

        // 2. get data from aws 
        Task<List<Resource>> awsTask;
        if (option.AwsconfigSupported)
        {
            awsTask = iamAdapter.ConvertConfigToResource(cancellation);
        }
        else
        {
            awsTask = iamAdapter.ConvertIAMToResource(cancellation);
        }

        // 3. merge data to db
        await Task.WhenAll(dbResourcesTask, awsTask);
        var dbResources = dbResourcesTask.Result;
        var awsResources = awsTask.Result;

        var insertDatas = GetExceptDatas(awsResources, dbResources);
        var deleteDatas = GetExceptDatas(dbResources, awsResources);
        var updateDatas = GetIntersectDatas(dbResources, awsResources);

        logger.LogInformation("{count} need to create, {count} need to delete, {count} need to update", insertDatas.Count, deleteDatas.Count, updateDatas.Count);

        if (insertDatas.Count == 0 && deleteDatas.Count == 0 && updateDatas.Count == 0)
        {
            return;
        }

        await resourceRepository.BulkWriteAsync(insertDatas, deleteDatas.Select(p => p.Id).ToList(), updateDatas, cancellation);
        await dapr.PublishEventAsync("resource-agent", "resources", new ResourceProcessorEvent(insertDatas, deleteDatas.Select(p => p.Id).ToList(), updateDatas), cancellation);
    }

    private static List<Resource> GetExceptDatas(List<Resource> first, List<Resource> second)
    {
        return first.ExceptBy(second.Select(d => d.Arn), d => d.Arn).ToList();
    }

    private static List<Resource> GetIntersectDatas(List<Resource> first, List<Resource> second)
    {
        return (from a in first
                join b in second on a.Id equals b.Id
                where a.ConfigurationItemCaptureTime != b.ConfigurationItemCaptureTime
                select b).ToList();
    }
}
