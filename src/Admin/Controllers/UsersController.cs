using AuthApi.Admin.Dto;
using AuthApi.Auth;
using AuthApi.Auth.Services.UserServices;
using AuthApi.Program;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OnRails.Extensions.ActionResult;

namespace AuthApi.Admin.Controllers;

[ApiController]
[Route($"{Routs.Admin}/[controller]")]
// [Authorize(Roles = Roles.Manager)]
public class UsersController(IUserManager userManager, IMapper mapper) : ControllerBase {
    [HttpGet]
    public async Task<List<GetUserRes>> GetAll(CancellationToken cancellationToken) {
        var users = await userManager.GetAllAsync(true, includeRoles: true, cancellationToken: cancellationToken);
        return users.Select(mapper.Map<GetUserRes>).ToList();
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<GetUserRes>> GetById(string userId, CancellationToken cancellationToken) {
        var user = await userManager.GetByIdAsync(userId, true, includeRoles: true,
            cancellationToken: cancellationToken);
        if (user is null) return NotFound();
        return Ok(mapper.Map<GetUserRes>(user));
    }

    [HttpDelete]
    public async Task<ActionResult> RemoveAllUsers() {
        var count = await userManager.RemoveAllUsersExceptRolesAsync(Roles.Administrator);
        return Ok(new { Count = count });
    }

    //TODO: check
    [HttpPost("Role/{userId}")]
    public Task<ActionResult> AddRoles(string userId, [FromBody] string[] roles, CancellationToken cancellationToken) {
        return userManager.AddRolesAsync(userId, roles, cancellationToken)
            .ReturnCreatedAtAction(nameof(GetById), new { userId });
    }

    [HttpDelete("Role/{userId}")]
    public Task<ActionResult> RemoveRoles(string userId, [FromBody] string[] roles, CancellationToken ct) {
        return userManager.RemoveRolesAsync(userId, roles, ct)
            .ReturnNoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> RemoveUser(string userId) {
        await userManager.DeleteByIdAsync(userId);
        return NoContent();
    }
}