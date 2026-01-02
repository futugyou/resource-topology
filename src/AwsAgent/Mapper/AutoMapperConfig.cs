
using Microsoft.Extensions.Logging.Abstractions;

namespace AwsAgent.Mapper;

public class AutoMapperConfig
{
    public static MapperConfiguration RegisterMapper()
    {
        return new MapperConfiguration(conf =>
        {
            conf.AddProfile(new MappingProfile());
        }, new NullLoggerFactory());
    }
}