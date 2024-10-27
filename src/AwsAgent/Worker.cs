using Domain;

namespace AwsAgent;

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
            var scope = _service.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IResourceRepository>();
            var resources = await repo.ListResources(stoppingToken);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                foreach (var resource in resources)
                {
                    _logger.LogInformation("Resource: {acc} {zone} {region} {id} {label}", resource.AccountID, resource.AvailabilityZone, resource.AwsRegion, resource.Id, resource.Label);
                }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
