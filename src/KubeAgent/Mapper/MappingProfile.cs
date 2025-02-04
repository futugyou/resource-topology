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

        // TODO: may be need mapping OwnerReference to Relationship
        CreateMap<List<Resource>, ResourceContracts.ResourceProcessorEvent>()
            .ForMember(dest => dest.EventID, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(_ => "Kubernetes"))
            .ForMember(dest => dest.InsertResources, opt => opt.MapFrom(src => src.Where(p => p.Operate == "Added").Select(p => new ResourceContracts.Resource
            {
                Id = p.UID,
                ResourceID = p.UID,
                ResourceName = p.Name,
                ResourceType = p.Kind,
                AccountID = p.Cluster,
                Region = p.Region,
                AvailabilityZone = p.Zone,
                Configuration = p.Configuration,
                ResourceCreationTime = p.ResourceCreationTime,
                ResourceHash = p.UID,
                Tags = p.Tags
            }).ToList()))
            .ForMember(dest => dest.DeleteResources, opt => opt.MapFrom(src => src.Where(p => p.Operate == "Deleted").Select(p => p.UID).ToList()))
            .ForMember(dest => dest.UpdateResources, opt => opt.MapFrom(src => src.Where(p => p.Operate == "Modified").Select(p => new ResourceContracts.Resource
            {
                Id = p.UID,
                ResourceID = p.UID,
                ResourceName = p.Name,
                ResourceType = p.Kind,
                AccountID = p.Cluster,
                Region = p.Region,
                AvailabilityZone = p.Zone,
                Configuration = p.Configuration,
                ResourceCreationTime = p.ResourceCreationTime,
                ResourceHash = p.UID,
                Tags = p.Tags
            }).ToList()));
    }
}