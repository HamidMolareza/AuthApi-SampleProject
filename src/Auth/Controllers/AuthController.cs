using System.Security.Claims;
using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Services;
using AuthApi.Program;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    IUnitOfWork unitOfWork,
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

        var tokens = await GenerateTokensAsync(user);
        return Ok(tokens);
    }

    private async Task<TokensRes> GenerateTokensAsync(User user) {
        var now = DateTime.UtcNow;
        var userId = await unitOfWork.UserManager.GetUserIdAsync(user);
        var sessionId = Guid.NewGuid();

        var refreshToken = unitOfWork.TokenManager.GenerateRefreshToken(sessionId.ToString(), now);
        var session = await CreateSessionAsync(userId, now, refreshToken, sessionId);

        var jwtToken = await unitOfWork.TokenManager.GenerateJwtAsync(userId, session.Id.ToString(), now);

        await unitOfWork.SaveChangesAsync();
        var tokens = new TokensRes(
            jwtToken.Value,
            jwtToken.Expire,
            refreshToken.Value,
            session.RefreshTokenExpiresAt
        );
        return tokens;
    }

    private async Task<Session> CreateSessionAsync(string userId, DateTime now, ITokenManager.Token refreshToken,
        Guid sessionId) {
        var session = new Session {
            Id = sessionId,
            IsRevoked = false,
            RefreshTokenExpiresAt = refreshToken.Expire,
            UserId = userId,
            CreatedAt = now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()!,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };
        unitOfWork.SessionManager.SetRefreshToken(session, refreshToken.Value);
        await unitOfWork.SessionManager.CreateAsync(session);
        return session;
    }

    private ActionResult BadRequest(IdentityResult result) {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokensRes>> Login(LoginReq req) {
        var result = await unitOfWork.SignInManager.PasswordSignInAsync(req.Email,
            req.Password, false, lockoutOnFailure: true);

        if (result.Succeeded) {
            var user = await unitOfWork.UserManager.FindByEmailAsync(req.Email);
            if (user is null) return Unauthorized();

            var tokens = await GenerateTokensAsync(user);
            return Ok(tokens);
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

        var sessionId = new Guid(inputRefreshToken.Sid);
        var session = await unitOfWork.SessionManager.GetByIdAsync(sessionId);
        if (session is null) return Unauthorized();

        var valid = await unitOfWork.SessionManager.ValidateRefreshTokenAsync(sessionId, req.RefreshToken);
        if (!valid) return Unauthorized();

        var tokens =
            await unitOfWork.TokenManager.GenerateTokensAsync(session.UserId, inputRefreshToken.Sid, DateTime.UtcNow);

        unitOfWork.SessionManager.SetRefreshToken(session, tokens.refresh.Value);
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

        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try {
            var result = await unitOfWork.UserManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
            if (!result.Succeeded) return BadRequest(result);

            await unitOfWork.SessionManager.RevokeAllExceptAsync(sessionGuid, userId);

            var (jwt, refresh) = await unitOfWork.TokenManager.GenerateTokensAsync(userId, sessionId, DateTime.UtcNow);
            _ = await unitOfWork.SessionManager.UpdateRefreshTokenAsync(sessionGuid, refresh.Value, refresh.Expire);

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new TokensRes(
                jwt.Value,
                jwt.Expire,
                refresh.Value,
                refresh.Expire
            ));
        }
        catch (Exception) {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout() {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(Claims.SessionId);
        if (userId is null || sessionId is null) return Unauthorized();

        var session = await unitOfWork.SessionManager.GetByIdAsync(new Guid(sessionId), userId);
        if (session is null) return Unauthorized();

        await unitOfWork.SessionManager.RemoveAsync(new Guid(sessionId), userId);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}