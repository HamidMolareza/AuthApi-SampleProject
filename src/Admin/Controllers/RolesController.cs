using AuthApi.Admin.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Admin.Controllers;

[ApiController]
[Route($"{Routs.Admin}/[controller]")]
public class RolesController(IUnitOfWork unitOfWork) : ControllerBase {
    [HttpGet]
    public Task<List<GetAllRolesRes>> GetAllRoles() {
        return unitOfWork.RoleManager.Roles.AsNoTracking()
            .Select(role => new GetAllRolesRes(role.Id, role.Name))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetRoleRes>> GetRoleById(string id) {
        var item = await unitOfWork.RoleManager.Roles.AsNoTracking()
            .FirstOrDefaultAsync(role => role.Id == id);
        if (item is null) return NotFound();

        return Ok(new GetRoleRes(item.Id, item.Name));
    }

    [HttpPost]
    public async Task<ActionResult> CreateRole(CreateRoleReq req) {
        var isExist = await unitOfWork.RoleManager.RoleExistsAsync(req.Name);
        if (isExist) return NoContent();

        var result = await unitOfWork.RoleManager.CreateAsync(new Role(req.Name));
        return result.Succeeded ? Created() : BadRequest(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateRole(string id, UpdateRole req) {
        var role = await unitOfWork.RoleManager.FindByIdAsync(id);
        if (role is null) return NoContent();

        var result = await unitOfWork.RoleManager.SetRoleNameAsync(role, req.NewName);
        return result.Succeeded ? NoContent() : BadRequest(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(string id) {
        var role = await unitOfWork.RoleManager.FindByIdAsync(id);
        if (role is null) return NoContent();

        var result = await unitOfWork.RoleManager.DeleteAsync(role);
        return result.Succeeded ? NoContent() : BadRequest(result);
    }

    private ActionResult BadRequest(IdentityResult result) {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(ModelState);
    }
}