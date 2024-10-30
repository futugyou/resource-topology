namespace AwsAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configuration = builder.Configuration;
        builder.Services.AddOptions<ServiceOption>().BindConfiguration(nameof(ServiceOption));

        builder.Services.AddScoped(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
            var mongoConnectionString = configuration.GetConnectionString("Mongodb");
            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient;
        });
        builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
        builder.Services.AddScoped<IResourceRelationshipRepository, ResourceRelationshipRepository>();

        var iamClient = new AmazonIdentityManagementServiceClient();
        builder.Services.AddScoped(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ServiceOption>>();
            var op = optionsMonitor.CurrentValue;
            var _credentials = new BasicAWSCredentials(op.AccessKeyId, op.SecretAccessKey);
            var region = RegionEndpoint.USEast1;
            if (op.Region.Length > 0)
            {
                region = RegionEndpoint.GetBySystemName(op.Region);
            }
            var iamClient = new AmazonIdentityManagementServiceClient(_credentials, region);
            return iamClient;
        });

        builder.Services.AddScoped<IAMResourceAdapter, AMResourceAdapter>();
        builder.Services.AddScoped<IResourceProcessor, ResourceProcessor>();

        builder.Services.AddHostedService<Worker>();

        string connectionString = configuration.GetConnectionString("Mongodb")!;
        string dbName = configuration["DBOption:DBName"]!;
        builder.Services.AddMongoDB<AwsContext>(connectionString, dbName);

        return builder;
    }
}
