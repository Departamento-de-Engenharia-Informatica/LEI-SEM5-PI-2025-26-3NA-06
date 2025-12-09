using AutoMapper;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.DTOs.User;

namespace ProjArqsi.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsGuid()))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Value))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Value.ToString()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.ConfirmationTokenExpiry, opt => opt.MapFrom(src => src.ConfirmationTokenExpiry))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }
    }
}
