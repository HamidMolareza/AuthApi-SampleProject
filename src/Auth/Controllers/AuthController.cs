using System.Security.Claims;
using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Program;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    IUnitOfWork unitOfWork,
    SignInManager<User> signInManager,
    ILogger<AuthController> logger) : ControllerBase {
    [HttpPost("register")]
    public async Task<ActionResult<TokensRes>> Register(RegisterUserReq userReq) {
        var user = new User {
            Email = userReq.Email,
            UserName = userReq.UserName
        };
        var result = await unitOfWork.UserManager.CreateAsync(user, userReq.Password);
        if (!result.Succeeded)
            return BadRequest(result);

        logger.LogInformation("User created a new account with password.");
        if (unitOfWork.UserManager.Options.SignIn.RequireConfirmedAccount) {
            return Ok(new {
                Message = "Registration successful. Please check your email to confirm your account."
            });
        }

        var userId = await unitOfWork.UserManager.GetUserIdAsync(user);
        var now = DateTime.UtcNow;
        var session = await CreateSessionAsync(userId, now);

        var jwtToken = await unitOfWork.TokenManager.GenerateJwtAsync(userId, session.Id.ToString(), now);
        return Ok(new TokensRes(
            jwtToken.Value,
            jwtToken.Expire,
            session.RefreshToken,
            session.RefreshTokenExpiresAt
        ));
    }

    private async Task<Session> CreateSessionAsync(string userId, DateTime now) {
        var sessionId = Guid.NewGuid();
        var refreshToken = unitOfWork.TokenManager.GenerateRefreshToken(sessionId.ToString(), now);
        var session = new Session {
            Id = sessionId,
            IsRevoked = false,
            RefreshToken = refreshToken.Value,
            RefreshTokenExpiresAt = refreshToken.Expire,
            UserId = userId,
            CreatedAt = now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()!,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };
        await unitOfWork.SessionManager.CreateAsync(session);
        await unitOfWork.SaveChangesAsync();
        return session;
    }

    private ActionResult BadRequest(IdentityResult result) {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokensRes>> Login(LoginReq req) {
        var result = await signInManager.PasswordSignInAsync(req.Email,
            req.Password, false, lockoutOnFailure: true);

        if (result.Succeeded) {
            var user = await unitOfWork.UserManager.FindByEmailAsync(req.Email);
            if (user is null) return Unauthorized();

            var now = DateTime.UtcNow;
            var session = await CreateSessionAsync(user.Id, now);

            var jwtToken = await unitOfWork.TokenManager.GenerateJwtAsync(user.Id, session.Id.ToString(), now);
            return Ok(new TokensRes(
                jwtToken.Value,
                jwtToken.Expire,
                session.RefreshToken,
                session.RefreshTokenExpiresAt
            ));
        }

        if (result.RequiresTwoFactor) {
            return Unauthorized(new { Message = "RequiresTwoFactor" });
        }

        if (result.IsLockedOut) {
            return Forbid();
        }

        return Unauthorized(new { Message = "Invalid login attempt." });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken(RefreshTokenReq req) {
        var inputRefreshToken = unitOfWork.TokenManager.ParseRefreshToken(req.RefreshToken);
        if (inputRefreshToken is null) return Unauthorized();

        var session = await unitOfWork.SessionManager.GetByIdAsync(new Guid(inputRefreshToken.Sid));
        if (session is null) return Unauthorized();

        if (session.RefreshToken != req.RefreshToken) return Unauthorized();
        if (session.RefreshTokenExpiresAt <= DateTime.UtcNow) return Unauthorized();

        var tokens =
            await unitOfWork.TokenManager.GenerateTokensAsync(session.UserId, inputRefreshToken.Sid, DateTime.UtcNow);

        session.RefreshToken = tokens.refresh.Value;
        session.RefreshTokenExpiresAt = tokens.refresh.Expire;
        await unitOfWork.SaveChangesAsync();

        return Ok(new TokensRes(
            tokens.jwt.Value,
            tokens.jwt.Expire,
            tokens.refresh.Value,
            tokens.refresh.Expire
        ));
    }

    [HttpPut("Password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordReq req) {
        if (req.CurrentPassword == req.NewPassword)
            return BadRequest(new { Message = "The new password can not equal with the current password." });

        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(Claims.SessionId);
        if (userId is null || sessionId is null) return Unauthorized();
        var sessionGuid = new Guid(sessionId);

        var user = await unitOfWork.UserManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var result = await unitOfWork.UserManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded) return BadRequest(result);

        await unitOfWork.SessionManager.RevokeAllExceptAsync(sessionGuid, userId);

        var (jwt, refresh) = await unitOfWork.TokenManager.GenerateTokensAsync(userId, sessionId, DateTime.UtcNow);
        await unitOfWork.SessionManager.UpdateRefreshTokenAsync(sessionGuid, refresh.Value, refresh.Expire);

        await unitOfWork.SaveChangesAsync();

        return Ok(new TokensRes(
            jwt.Value,
            jwt.Expire,
            refresh.Value,
            refresh.Expire
        ));
    }
}