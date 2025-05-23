using AutoMapper;
using CashFlow.Application.Dtos;
using CashFlow.CrossCutting;

namespace CashFlow.Identity.Models.Requests;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserRegistrationRequest, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Geralmente gerado posteriormente
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => new List<RoleTypes>())) // Lista vazia padrão
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
    }
}