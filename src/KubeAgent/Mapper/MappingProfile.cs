namespace KubeAgent.Mapper;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        // WatcherInfo -> MonitoringContext
        CreateMap<WatcherInfo, MonitoringContext>();

        // MonitoringContext -> WatcherInfo
        CreateMap<MonitoringContext, WatcherInfo>()
            .ForMember(dest => dest.ResourceId, opt => opt.MapFrom(src => src.ResourceId()));

        // MonitoredResource -> MonitoringContext
        CreateMap<MonitoredResource, MonitoringContext>();

        // MonitoringContext -> MonitoredResource
        CreateMap<MonitoringContext, MonitoredResource>();
    }
}