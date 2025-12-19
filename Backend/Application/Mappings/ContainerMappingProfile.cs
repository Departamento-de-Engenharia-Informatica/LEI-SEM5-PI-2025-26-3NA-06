using AutoMapper;
using ProjArqsi.Domain.ContainerAggregate;
using ProjArqsi.Application.DTOs;

namespace ProjArqsi.Application.Mappings
{
    public class ContainerMappingProfile : Profile
    {
        public ContainerMappingProfile()
        {
            CreateMap<Container, ContainerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value.ToString()))
                .ForMember(dest => dest.IsoCode, opt => opt.MapFrom(src => src.IsoCode.Value))
                .ForMember(dest => dest.IsHazardous, opt => opt.MapFrom(src => src.IsHazardous))
                .ForMember(dest => dest.CargoType, opt => opt.MapFrom(src => src.CargoType.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Text));
        }
    }
}
