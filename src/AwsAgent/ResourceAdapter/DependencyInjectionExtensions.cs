

using Amazon;
using Amazon.IdentityManagement;
using Amazon.Runtime;

namespace ResourceAdapter;


public static class DependencyInjectionExtensions
{
    public static IServiceCollection ResourceAdapterRegistration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return services.ResourceAdapterRegistration(configuration);
    }

    public static IServiceCollection ResourceAdapterRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AwsOption>(configuration.GetSection("AwsOption"));

        var iamClient = new AmazonIdentityManagementServiceClient();
        services.AddScoped(sp =>
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

        services.AddScoped<IAMResourceAdapter, AMResourceAdapter>();
        return services;
    }

}
