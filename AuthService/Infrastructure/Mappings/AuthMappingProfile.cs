using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using AutoMapper;

namespace AuthService.Infrastructure.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterRequestDto, User>()
            .ConstructUsing(src => new User(src.Username, "", src.Email, src.Role));
        
        CreateMap<User, LoginResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Token, opt => opt.Ignore()); // set manually
    }
}