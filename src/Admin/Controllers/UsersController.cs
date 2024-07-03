using AuthApi.Admin.Dto;
using AuthApi.Auth;
using AuthApi.Auth.Entities;
using AuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Admin.Controllers;

[ApiController]
[Route($"{Routs.Admin}/[controller]")]
// [Authorize(Roles = Roles.Manager)]
public class UsersController(IUnitOfWork unitOfWork) : ControllerBase {
    [HttpGet]
    public Task<List<GetAllUsersRes>> GetAll() {
        return unitOfWork.UserManager.UsersWithRoles.AsNoTracking()
            .Select(user => new GetAllUsersRes(user.Id, user.UserName, user.Email,
                user.EmailConfirmed, user.UserRoles.Select(userRole => userRole.Role.Name!)))
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserByIdRes>> GetById(string id) {
        var user = await unitOfWork.UserManager.UsersWithRoles.AsNoTracking()
            .Where(user => user.Id == id)
            .FirstOrDefaultAsync();
        if (user is null) return NotFound();
        return Ok(new GetUserByIdRes(user.Id, user.UserName, user.Email,
            user.EmailConfirmed, user.UserRoles.Select(userRole => userRole.Role.Name!)));
    }

    [HttpDelete]
    public async Task<ActionResult> RemoveAllUsers() {
        var nonAdminUsers = unitOfWork.UserManager.UsersWithRoles
            .Where(user => user.UserRoles.All(userRole => userRole.Role.Name != Roles.Administrator))
            .ToArray();

        var deletedUserEmails = nonAdminUsers.Select(user => new {
            user.Id,
            user.UserName,
            user.Email,
        });

        unitOfWork.UserManager.RemoveRanges(nonAdminUsers);
        await unitOfWork.SaveChangesAsync();

        return Ok(new { DeletedUsers = deletedUserEmails });
    }

    [HttpPost("Role/{id}")]
    public async Task<ActionResult> AddRole(string id, [FromBody] string[] roles) {
        var user = await unitOfWork.UserManager.UsersWithRoles
            .FirstOrDefaultAsync(user => user.Id == id);
        if (user is null) return NotFound();

        var newRoles = roles.Where(role =>
                user.UserRoles.All(userRole => userRole.Role.Name != role))
            .ToList();
        if (newRoles.Count == 0) return NoContent();

        var result = await unitOfWork.UserManager.AddToRolesAsync(user, newRoles);
        return result.Succeeded ? NoContent() : BadRequest(result);
    }

    [HttpDelete("Role/{id}")]
    public async Task<ActionResult> RemoveRole(string id, [FromBody] string[] roles) {
        var user = await unitOfWork.UserManager.UsersWithRoles
            .FirstOrDefaultAsync(user => user.Id == id);
        if (user is null) return NotFound();

        var existRoles = roles.Where(role =>
                user.UserRoles.Any(userRole => userRole.Role.Name == role))
            .ToList();
        if (existRoles.Count == 0) return NoContent();

        var result = await unitOfWork.UserManager.RemoveFromRolesAsync(user, existRoles);
        return result.Succeeded ? NoContent() : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveUser(string id) {
        var user = await unitOfWork.UserManager.UsersWithRoles.FirstOrDefaultAsync();
        if (user is null) return NoContent();

        if (user.UserRoles.Any(role => role.Role.Name == Roles.Administrator))
            return BadRequest(new { Message = "Can not remove an admin use.r" });

        var result = await unitOfWork.UserManager.DeleteAsync(new User { Id = id });
        return result.Succeeded ? NoContent() : BadRequest(result);
    }

    private ActionResult BadRequest(IdentityResult result) {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(ModelState);
    }
}