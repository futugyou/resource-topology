namespace AwsAgent.Processor;

public abstract class AbstractResourceProcessor(ILogger<AbstractResourceProcessor> logger) : IResourceProcessor
{
    public async Task ProcessingData(CancellationToken cancellation)
    {
        Task<(List<Resource> resources, List<ResourceRelationship> resourceShips)> dbDataTask = GetResourcesFromDB(cancellation);
        Task<(List<Resource> resources, List<ResourceRelationship> resourceShips)> awsDataTask = GetResourcesFromAWS(cancellation);
        await Task.WhenAll(dbDataTask, awsDataTask);

        (List<Resource> resources, List<ResourceRelationship> resourceShips) = await GetAdditionalResources(awsDataTask.Result.resources, awsDataTask.Result.resourceShips, cancellation);
        (resources, resourceShips) = await MergeResources(resources, resourceShips, cancellation);

        var record = await DifferentialResourcesData(dbDataTask.Result.resources, dbDataTask.Result.resourceShips, resources, resourceShips, cancellation);
        if (!record.HasChange())
        {
            return;
        }

        logger.LogInformation("{count} resources need to create, {count} resources need to delete, {count} resources need to update", record.InsertDatas.Count, record.DeleteDatas.Count, record.UpdateDatas.Count);
        logger.LogInformation("{count} ships need to create, {count} ships need to delete,", record.InsertShipDatas.Count, record.DeleteShipDatas.Count);

        await SaveDifferentialDatas(record, cancellation);
        await SendResourceProcessingEvent(record, cancellation);
    }

    protected abstract Task<(List<Resource>, List<ResourceRelationship>)> GetResourcesFromAWS(CancellationToken cancellation);

    protected abstract Task<(List<Resource>, List<ResourceRelationship>)> GetResourcesFromDB(CancellationToken cancellation);

    // additional resources and relations
    protected abstract Task<(List<Resource>, List<ResourceRelationship>)> GetAdditionalResources(List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation);

    // merge resource config and hash
    protected abstract Task<(List<Resource>, List<ResourceRelationship>)> MergeResources(List<Resource> resources, List<ResourceRelationship> ships, CancellationToken cancellation);

    protected abstract Task<DifferentialResourcesRecord> DifferentialResourcesData(List<Resource> dbResources, List<ResourceRelationship> dbShips, List<Resource> awsResources, List<ResourceRelationship> awsShips, CancellationToken cancellation);

    protected abstract Task SaveDifferentialDatas(DifferentialResourcesRecord record, CancellationToken cancellation);

    protected abstract Task SendResourceProcessingEvent(DifferentialResourcesRecord record, CancellationToken cancellation);
}

public record DifferentialResourcesRecord(List<Resource> InsertDatas, List<Resource> DeleteDatas, List<Resource> UpdateDatas, List<ResourceRelationship> InsertShipDatas, List<ResourceRelationship> DeleteShipDatas)
{
    public bool HasChange() => InsertDatas.Count != 0 || DeleteDatas.Count != 0 || UpdateDatas.Count != 0 || InsertShipDatas.Count != 0 || DeleteShipDatas.Count != 0;
}