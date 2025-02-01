namespace AwsAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddServiceDefaults();
        builder.Services.AddAutoMapper(typeof(Program));
        AutoMapperConfig.RegisterMapper();

        var daprClientBuilder = new DaprClientBuilder();
        DaprClient daprClient = daprClientBuilder.Build();
        while (!daprClient.CheckHealthAsync().Result)
        {
            Console.WriteLine("wait for aws agent dapr health check.");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        builder.Services.AddSingleton(daprClient);

        var configuration = builder.Configuration;
        configuration.AddDaprConfigurationStore("aws-agent-config", [], daprClient, TimeSpan.FromSeconds(20));
        configuration.AddStreamingDaprConfigurationStore("aws-agent-config", [], daprClient, TimeSpan.FromSeconds(20));
        configuration.AddDaprSecretStore("aws-agent-secret", daprClient, TimeSpan.FromSeconds(10));

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

        builder.Services.AddScoped<IResourceAdapterWrapper, ResourceAdapterWrapper>();

        builder.Services.AddScoped(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceOption>>().Value;
            var serviceCollection = new List<IResourceAdapter>();

            if (options.AwsconfigSupported)
            {
                serviceCollection.Add(sp.GetRequiredService<AwsConfigAdapter>());
            }
            else
            {
                serviceCollection.Add(sp.GetRequiredService<AwsIAMUserAdapter>());
                serviceCollection.Add(sp.GetRequiredService<AwsIAMPolicyAdapter>());
            }

            return serviceCollection.AsEnumerable();
        });

        builder.Services.AddScoped<AwsIAMUserAdapter>();
        builder.Services.AddScoped<AwsIAMPolicyAdapter>();
        builder.Services.AddScoped<AwsConfigAdapter>();

        builder.Services.AddKeyedScoped<IResourceProcessor, ResourceProcessor>("Normal");
        builder.Services.AddKeyedScoped<IResourceProcessor, WorkflowProcessor>("Workflow");
        // builder.Services.AddDaprClient();

        builder.Services.AddHostedService<Worker>();

        // We do not want use efcore for this project.
        // string connectionString = configuration.GetConnectionString("Mongodb")!;
        // string dbName = configuration["DBOption:DBName"]!;
        // builder.Services.AddMongoDB<AwsContext>(connectionString, dbName);

        builder.Services.AddDaprWorkflow(options =>
        {
            options.RegisterWorkflow<ResourceProcessorWorkflow>();
            options.RegisterActivity<GetDatabaseResourceActivity>();
            options.RegisterActivity<GetAwsResourceActivity>();
            options.RegisterActivity<IncrementResourceActivity>();
            options.RegisterActivity<SaveResourceActivity>();
            options.RegisterActivity<SendResourceProcessingEventActivity>();
        });

        return builder;
    }
}
