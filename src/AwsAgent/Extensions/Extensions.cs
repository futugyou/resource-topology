namespace AwsAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<DBOption>().BindConfiguration(nameof(DBOption));
        builder.Services.AddOptions<AwsOption>().BindConfiguration(nameof(AwsOption));

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

        var iamClient = new AmazonIdentityManagementServiceClient();
        builder.Services.AddScoped(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<AwsOption>>();
            var op = optionsMonitor.CurrentValue;
            var _credentials = new BasicAWSCredentials(op.AwsAccessKeyId, op.AwsSecretAccessKey);
            var region = RegionEndpoint.USEast1;
            if (op.Region.Length > 0)
            {
                region = RegionEndpoint.GetBySystemName(op.Region);
            }
            var iamClient = new AmazonIdentityManagementServiceClient(_credentials, region);
            return iamClient;
        });

        builder.Services.AddScoped<IAMResourceAdapter, AMResourceAdapter>();
        builder.Services.AddHostedService<Worker>();

        return builder;
    }
}
