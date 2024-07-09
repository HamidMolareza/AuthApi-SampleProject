using AuthApi.Admin.Dto;
using AuthApi.Auth.Entities;
using AutoMapper;

namespace AuthApi.Admin;

public class AdminMappingProfile : Profile {
    public AdminMappingProfile() {
        CreateMap<Role, GetRoleRes>();

        CreateMap<User, GetUserRes>()
            .ForMember(d => d.Roles,
                opt =>
                    opt.MapFrom(src => src.UserRoles.Select(userRole => userRole.Role.Name)));
    }
}