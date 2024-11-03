

namespace AwsAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configuration = builder.Configuration;
        builder.Services.AddOptions<ServiceOption>().BindConfiguration(nameof(ServiceOption));
        builder.Services.PostConfigure<ServiceOption>(op =>
        {
            // when app running in github action, it will do worker once.
            var githubAction = Environment.GetEnvironmentVariable(Const.GITHUB_ACTIONS_ENV);
            if (githubAction == "true")
            {
                op.RunSingle = true;
            }
        });

        builder.Services.AddScoped(sp =>
        {
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
            var mongoConnectionString = configuration.GetConnectionString(Const.MONGODB_SECTION);
            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient;
        });
        builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
        builder.Services.AddScoped<IResourceRelationshipRepository, ResourceRelationshipRepository>();

        builder.Services.AddDefaultAWSOptions(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ServiceOption>>();
            var options = optionsMonitor.CurrentValue;
            var _credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);
            var region = RegionEndpoint.USEast1;
            if (options.Region.Length > 0)
            {
                region = RegionEndpoint.GetBySystemName(options.Region);
            }

            return new AWSOptions()
            {
                Credentials = _credentials,
                Region = region,
            };
        });

        builder.Services.AddAWSService<IAmazonIdentityManagementService>();
        builder.Services.AddAWSService<IAmazonConfigService>();

        //TODO: handle multiple injection
        builder.Services.AddScoped<IResourceAdapter, AwsIamAdapter>();
        builder.Services.AddScoped<IResourceAdapter, AwsConfigAdapter>();
        builder.Services.AddScoped<IResourceProcessor, ResourceProcessor>();
        builder.Services.AddDaprClient();
        
        builder.Services.AddHostedService<Worker>();

        // We do not want use efcore for this project.
        // string connectionString = configuration.GetConnectionString("Mongodb")!;
        // string dbName = configuration["DBOption:DBName"]!;
        // builder.Services.AddMongoDB<AwsContext>(connectionString, dbName);

        return builder;
    }
}
