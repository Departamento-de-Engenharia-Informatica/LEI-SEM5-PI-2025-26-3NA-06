using AutoMapper;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Application.DTOs;

namespace ProjArqsi.Application.Mappings
{
    public class VesselTypeMappingProfile : Profile
    {
        public VesselTypeMappingProfile()
        {
            CreateMap<VesselType, VesselTypeDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsGuid()))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.TypeName.Value))
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.TypeDescription.Value))
                .ForMember(dest => dest.TypeCapacity, opt => opt.MapFrom(src => src.TypeCapacity.Value))
                .ForMember(dest => dest.MaxRows, opt => opt.MapFrom(src => src.MaxRows.Value))
                .ForMember(dest => dest.MaxBays, opt => opt.MapFrom(src => src.MaxBays.Value))
                .ForMember(dest => dest.MaxTiers, opt => opt.MapFrom(src => src.MaxTiers.Value));
        }
    }
}
