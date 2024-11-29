namespace AwsAgent.Processor;

public abstract class AbstractResourceProcessor(ILogger<AbstractResourceProcessor> logger) : IResourceProcessor
{
    public async Task ProcessingData(CancellationToken cancellation)
    {
        Task<ResourceAndShip> dbDataTask = GetResourcesFromDB(cancellation);
        Task<ResourceAndShip> awsDataTask = GetResourcesFromAWS(cancellation);
        await Task.WhenAll(dbDataTask, awsDataTask);

        var record = await DifferentialResourcesData(dbDataTask.Result.Resources, dbDataTask.Result.Ships,
            awsDataTask.Result.Resources, awsDataTask.Result.Ships, cancellation);
        if (!record.HasChange())
        {
            return;
        }

        logger.LogInformation("{count} resources need to create, {count} resources need to delete, {count} resources need to update", record.InsertDatas.Count, record.DeleteDatas.Count, record.UpdateDatas.Count);
        logger.LogInformation("{count} ships need to create, {count} ships need to delete,", record.InsertShipDatas.Count, record.DeleteShipDatas.Count);

        await SaveDifferentialDatas(record, cancellation);
        await SendResourceProcessingEvent(record, cancellation);
    }

    protected abstract Task<ResourceAndShip> GetResourcesFromAWS(CancellationToken cancellation);

    protected abstract Task<ResourceAndShip> GetResourcesFromDB(CancellationToken cancellation);

    protected abstract Task<DifferentialResourcesRecord> DifferentialResourcesData(List<Resource> dbResources, List<ResourceRelationship> dbShips, List<Resource> awsResources, List<ResourceRelationship> awsShips, CancellationToken cancellation);

    protected abstract Task SaveDifferentialDatas(DifferentialResourcesRecord record, CancellationToken cancellation);

    protected abstract Task SendResourceProcessingEvent(DifferentialResourcesRecord record, CancellationToken cancellation);
}
