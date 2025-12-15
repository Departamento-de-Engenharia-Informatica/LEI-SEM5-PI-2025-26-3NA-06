using AutoMapper;
using ProjArqsi.Domain.StorageAreaAggregate;
using ProjArqsi.Application.DTOs.StorageArea;

namespace ProjArqsi.Application.Mappings
{
    public class StorageAreaMappingProfile : Profile
    {
        public StorageAreaMappingProfile()
        {
            CreateMap<StorageArea, StorageAreaDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Name.Value))
                .ForMember(dest => dest.AreaType, opt => opt.MapFrom(src => src.AreaType.Value.ToString()))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location.Description))
                .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.MaxCapacity.Value))
                .ForMember(dest => dest.ServedDockIds, opt => opt.MapFrom(src => src.ServedDocks.Value.Select(dockId => dockId.Value).ToList()))
                .ForMember(dest => dest.ServesEntirePort, opt => opt.MapFrom(src => src.ServesEntirePort));
        }
    }
}
