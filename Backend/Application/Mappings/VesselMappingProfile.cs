using AutoMapper;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Application.DTOs;

namespace ProjArqsi.Application.Mappings
{
    public class VesselMappingProfile : Profile
    {
        public VesselMappingProfile()
        {
            CreateMap<Vessel, VesselDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value.ToString()))
                .ForMember(dest => dest.Imo, opt => opt.MapFrom(src => src.IMO.Number))
                .ForMember(dest => dest.VesselName, opt => opt.MapFrom(src => src.VesselName.Name))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity.Value))
                .ForMember(dest => dest.Rows, opt => opt.MapFrom(src => src.Rows.Value))
                .ForMember(dest => dest.Bays, opt => opt.MapFrom(src => src.Bays.Value))
                .ForMember(dest => dest.Tiers, opt => opt.MapFrom(src => src.Tiers.Value))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length.Value))
                .ForMember(dest => dest.VesselTypeId, opt => opt.MapFrom(src => src.VesselTypeId.Value.ToString()))
                .ForMember(dest => dest.VesselTypeName, opt => opt.Ignore()); // Set manually in service
        }
    }
}
