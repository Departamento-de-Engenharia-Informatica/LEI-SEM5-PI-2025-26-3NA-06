using AutoMapper;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Application.DTOs.Dock;

namespace ProjArqsi.Application.Mappings
{
    public class DockMappingProfile : Profile
    {
        public DockMappingProfile()
        {
            CreateMap<Dock, DockDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
                .ForMember(dest => dest.DockName, opt => opt.MapFrom(src => src.DockName.Value))
                .ForMember(dest => dest.LocationDescription, opt => opt.MapFrom(src => src.Location.Description))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length.Value))
                .ForMember(dest => dest.Depth, opt => opt.MapFrom(src => src.Depth.Value))
                .ForMember(dest => dest.MaxDraft, opt => opt.MapFrom(src => src.MaxDraft.Value))
                .ForMember(dest => dest.AllowedVesselTypeIds, opt => opt.MapFrom(src => src.AllowedVesselTypes.VesselTypeIds));
        }
    }
}
