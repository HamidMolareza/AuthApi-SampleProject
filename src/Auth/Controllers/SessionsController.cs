using System.Security.Claims;
using AuthApi.Auth.Dto;
using AuthApi.Auth.Services.Session;
using AuthApi.Program;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SessionsController(IMapper mapper, ISessionManager sessionManager, IUnitOfWork unitOfWork)
    : ControllerBase {
    [HttpGet]
    public async Task<ActionResult<List<GetSessionRes>>> GetAll(CancellationToken ct) {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var sessions = await sessionManager.GetAllAsync(true, userId: userId, cancellationToken: ct);
        return sessions.Select(mapper.Map<GetSessionRes>).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetSessionRes>> GetById(string id, CancellationToken ct) {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var sessions = await sessionManager.GetByIdAsync(new Guid(id), userId, ct);
        if (sessions is null) return NotFound();

        var result = mapper.Map<GetSessionRes>(sessions);
        return Ok(result);
    }

    [HttpGet("Current")]
    public async Task<ActionResult<GetSessionRes>> GetCurrent(CancellationToken ct) {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(JwtClaims.SessionId);
        if (userId is null || sessionId is null) return Unauthorized();

        var sessions = await sessionManager.GetByIdAsync(new Guid(sessionId), userId, cancellationToken: ct);
        if (sessions is null) return Unauthorized();

        var result = mapper.Map<GetSessionRes>(sessions);
        return Ok(result);
    }

    [HttpPost("TerminateOther")]
    public async Task<ActionResult> TerminateOther() {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(JwtClaims.SessionId);
        if (userId is null || sessionId is null) return Unauthorized();

        await sessionManager.RevokeAllExceptAsync(userId, new Guid(sessionId));
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}