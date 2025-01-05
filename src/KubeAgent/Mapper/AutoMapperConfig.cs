namespace KubeAgent.Mapper;

public class AutoMapperConfig
{
    public static MapperConfiguration RegisterMapper()
    {
        return new MapperConfiguration(conf =>
        {
            conf.AddProfile(new MappingProfile());
        });
    }
}