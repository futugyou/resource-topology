using Domain;
using MongoDB.Bson.Serialization.Conventions;

namespace Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection ServiceRegistration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return services.ServiceRegistration(configuration);
    }

    public static IServiceCollection ServiceRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<DBOption>(configuration.GetSection("DBOption"));
        services.AddScoped(sp =>
        {
            var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);
            var mongoConnectionString = configuration.GetConnectionString("Mongodb");
            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient;
        });
        services.AddScoped<IResourceRepository, ResourceRepository>();
        return services;
    }

}
