using AutoMapper;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.DTOs.User;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.AsGuid()))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username.Value))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Value.ToString()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => new UserId(src.Id)))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => new Username(src.Username)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => new Role(Enum.Parse<RoleType>(src.Role))))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new Email(src.Email)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
  }
}
