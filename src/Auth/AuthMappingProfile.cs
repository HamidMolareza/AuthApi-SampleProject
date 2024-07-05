using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AutoMapper;

namespace AuthApi.Auth;

public class AuthMappingProfile : Profile {
    public AuthMappingProfile() {
        CreateMap<Session, GetSessionsRes>();
    }
}