using AuthApi.Admin.Dto;
using AuthApi.Auth.Entities;
using AutoMapper;

namespace AuthApi.Admin;

public class AdminMappingProfile : Profile {
    public AdminMappingProfile() {
        CreateMap<Role, GetRoleRes>();
    }
}