using AuthApi.Admin.Dto;
using AuthApi.Program;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Admin.Controllers;

[ApiController]
[Route($"{Routs.Admin}/[controller]")]
public class RolesController(IUnitOfWork unitOfWork, IMapper mapper) : ControllerBase {
    [HttpGet]
    public async Task<List<GetRoleRes>> GetAllRoles() {
        var roles = await unitOfWork.RoleManager
            .GetAllAsync(false, false, true);
        return roles.Select(mapper.Map<GetRoleRes>).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetRoleRes>> GetRoleById(string id) {
        var item = await unitOfWork.RoleManager
            .GetByIdAsync(id, false, false, true);

        if (item is null) return NotFound();
        return Ok(mapper.Map<GetRoleRes>(item));
    }

    [HttpPost]
    public async Task<ActionResult<GetRoleRes>> CreateRole(CreateRoleReq req) {
        var newRoles = await unitOfWork.RoleManager.AddNewNamesAsync(req.Names);
        var added = await unitOfWork.SaveChangesAsync();

        var newRoleNames = newRoles.Select(role => role.Name);
        return Ok(new { Count = added, New = newRoleNames });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateRole(string id, UpdateRole req) {
        var role = await unitOfWork.RoleManager.FindByIdAsync(id);
        if (role is null) return NotFound();

        var newRole = await unitOfWork.RoleManager.FindByNameAsync(req.NewName);
        if (newRole is not null) return Conflict();

        var result = await unitOfWork.RoleManager.SetRoleNameAsync(role, req.NewName);
        if (!result.Succeeded)
            return BadRequest(result);

        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(string id) {
        var role = await unitOfWork.RoleManager.FindByIdAsync(id);
        if (role is null) return NoContent();

        var result = await unitOfWork.RoleManager.DeleteAsync(role);
        return result.Succeeded ? NoContent() : BadRequest(result);
    }

    [HttpDelete("names")]
    public async Task<ActionResult> DeleteByNames(DeleteByNamesReq req) {
        var roles = await unitOfWork.RoleManager.GetByNamesAsync(req.Names, false, false, false);
        unitOfWork.RoleManager.RemoveRanges(roles);
        var deleted = await unitOfWork.SaveChangesAsync();

        var deletedRoles = roles.Select(role => role.Name);
        return Ok(new { Count = deleted, Deleted = deletedRoles });
    }

    private ActionResult BadRequest(IdentityResult result) {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(ModelState);
    }
}