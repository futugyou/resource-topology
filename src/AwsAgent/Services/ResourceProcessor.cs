namespace AwsAgent.Services;

public class ResourceProcessor(ILogger<ResourceProcessor> logger, IResourceRepository resourceRepository, IAMResourceAdapter iamAdapter) : IResourceProcessor
{
    async Task IResourceProcessor.ProcessingData(CancellationToken cancellation)
    {
        // 1. get data from db 
        var dbResourcesTask = resourceRepository.ListResources(cancellation);

        // 2. get data from aws 
        var awsResourcesTask = iamAdapter.ConvertIAMToResource(cancellation);

        // 3. merge data to db
        await Task.WhenAll(dbResourcesTask, awsResourcesTask);
        var dbResources = dbResourcesTask.Result;
        var awsResources = awsResourcesTask.Result;
        // TODO: 3.1 handler new data first, in the future, we will handle updates and deletions
        var insertDatas = GetInsertDatas(dbResources, awsResources);
        logger.LogInformation("{count} need to create", insertDatas.Count());
        if (insertDatas.Count != 0)
        {
            await resourceRepository.CreateResources(insertDatas, cancellation);
        }
    }

    private static List<Resource> GetInsertDatas(List<Resource> dbDatas, List<Resource> awsDatas)
    {
        return awsDatas.Except(dbDatas, new ResourceComparer()).ToList();
    }

    public class ResourceComparer : IEqualityComparer<Resource>
    {
        public bool Equals(Resource? x, Resource? y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.Id == y.Id;
        }

        public int GetHashCode(Resource obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}