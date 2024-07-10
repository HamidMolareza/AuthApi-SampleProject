using AuthApi.Admin.Dto;
using AuthApi.Auth.Services.Role;
using AuthApi.Program;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OnRails.Extensions.ActionResult;

namespace AuthApi.Admin.Controllers;

[ApiController]
[Route($"{Routs.Admin}/[controller]")]
public class RolesController(IMapper mapper, IRoleManager roleManager) : ControllerBase {
    [HttpPost]
    public async Task<ActionResult<List<GetRoleRes>>> CreateRole(CreateRoleReq req) {
        var createdRoles = await roleManager.CreateRolesAsync(req.Names);
        return createdRoles.Select(mapper.Map<GetRoleRes>).ToList();
    }

    [HttpGet]
    public async Task<List<GetRoleRes>> GetAllRoles() {
        var roles = await roleManager.GetAllAsync(true);
        return roles.Select(mapper.Map<GetRoleRes>).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetRoleRes>> GetRoleById(string id) {
        var item = await roleManager.GetByIdAsync(id, true);

        if (item is null) return NotFound();
        return Ok(mapper.Map<GetRoleRes>(item));
    }

    [HttpPut("{id}")]
    public Task<ActionResult> UpdateRole(string id, UpdateRole req) {
        return roleManager.UpdateByIdAsync(id, req.NewName)
            .ReturnNoContent();
    }

    [HttpDelete("{id}")]
    public Task<ActionResult> DeleteRole(string id) {
        return roleManager.DeleteByIdAsync(id)
            .ReturnNoContent();
    }

    [HttpDelete("names")]
    public async Task<ActionResult> DeleteByNames(DeleteByNamesReq req) {
        var deletedRoles = await roleManager.DeleteByNamesAsync(req.Names);

        var deletedRoleNames = deletedRoles.Select(role => role.Name);
        return Ok(new { Count = deletedRoles.Count, Deleted = deletedRoleNames });
    }
}