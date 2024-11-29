namespace AwsAgent.Mapper;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<DifferentialResourcesRecord, ResourceContracts.ResourceProcessorEvent>()
            .ForMember(dest => dest.InsertResources, opt => opt.MapFrom(src => src.InsertDatas))
            .ForMember(dest => dest.DeleteResources, opt => opt.MapFrom(src => src.DeleteDatas.Select(p => p.Id)))
            .ForMember(dest => dest.UpdateResources, opt => opt.MapFrom(src => src.UpdateDatas))
            .ForMember(dest => dest.InsertShips, opt => opt.MapFrom(src => src.InsertShipDatas))
            .ForMember(dest => dest.DeleteShips, opt => opt.MapFrom(src => src.DeleteShipDatas.Select(p => p.Id)))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(_ => "Aws"));

        CreateMap<Resource, ResourceContracts.Resource>()
            .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.AwsRegion))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToDictionary(tag => tag.Key, tag => tag.Value)));


        CreateMap<ResourceRelationship, ResourceContracts.ResourceRelationship>()
            .ForMember(dest => dest.Relation, opt => opt.MapFrom(src => src.Label));
    }
}