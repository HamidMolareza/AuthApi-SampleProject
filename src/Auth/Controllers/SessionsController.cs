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

    [HttpGet("Current")]
    public async Task<ActionResult<GetSessionsRes>> GetCurrent() {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(Claims.SessionId);
        if (userId is null || sessionId is null) return Unauthorized();

        var sessions = await unitOfWork.SessionManager.GetByIdAsync(new Guid(sessionId), userId);
        if (sessions is null) return Unauthorized();
        return Ok(new GetSessionsRes(sessions.Id, sessions.IpAddress, sessions.UserAgent, sessions.IsRevoked,
            sessions.CreatedAt));
    }

    [HttpPost("TerminateOther")]
    public async Task<ActionResult> TerminateOther() {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(Claims.SessionId);
        if (userId is null || sessionId is null) return Unauthorized();

        await unitOfWork.SessionManager.RevokeAllExceptAsync(new Guid(sessionId), userId);
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}