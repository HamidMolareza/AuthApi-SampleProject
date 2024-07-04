using System.Security.Claims;
using AuthApi.Auth.Dto;
using AuthApi.Program;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//TODO: Use automapper
namespace AuthApi.Auth.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SessionsController(IUnitOfWork unitOfWork) : ControllerBase {
    [HttpGet]
    public async Task<ActionResult<List<GetSessionsRes>>> GetAll() {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var sessions = await unitOfWork.SessionManager.GetAllAsync(userId);
        return sessions.Select(s => new GetSessionsRes(s.Id, s.IpAddress, s.UserAgent, s.IsRevoked, s.CreatedAt))
            .ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetSessionsRes>> GetById(string id) {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var sessions = await unitOfWork.SessionManager.GetByIdAsync(new Guid(id), userId);
        if (sessions is null) return NotFound();
        return Ok(new GetSessionsRes(sessions.Id, sessions.IpAddress, sessions.UserAgent, sessions.IsRevoked,
            sessions.CreatedAt));
    }
}