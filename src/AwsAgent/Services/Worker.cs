namespace AwsAgent.Services;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _service;

    public Worker(ILogger<Worker> logger, IServiceProvider service)
    {
        _logger = logger;

        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {
                await HandleAwsResourceAgent(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, ex.Message);
            }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleAwsResourceAgent(CancellationToken stoppingToken)
    {
        var scope = _service.CreateScope();

        // 1. get data from db
        var repo = scope.ServiceProvider.GetRequiredService<IResourceRepository>();
        var dbResourcesTask = repo.ListResources(stoppingToken);

        // 2. get data from aws
        var adapter = scope.ServiceProvider.GetRequiredService<IAMResourceAdapter>();
        var awsResourcesTask = adapter.ConvertIAMToResource(stoppingToken);

        // 3. merge data to db
        await Task.WhenAll(dbResourcesTask, awsResourcesTask);
        var dbResources = dbResourcesTask.Result;
        var awsResources = awsResourcesTask.Result;
        // TODO: 3.1 handler new data first, in the future, we will handle updates and deletions
        var insertDatas = GetInsertDatas(dbResources, awsResources);
        if (insertDatas.Count != 0)
        {
            await repo.CreateResources(insertDatas, stoppingToken);
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
