using System.Security.Claims;
using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Services.Session;
using AuthApi.Auth.Services.Token;
using AuthApi.Auth.Services.UserServices;
using AuthApi.Program;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    ILogger<AuthController> logger,
    SignInManager<User> signInManager,
    IUserManager userManager,
    ITokenManager tokenManager,
    IUnitOfWork unitOfWork,
    ISessionManager sessionManager) : ControllerBase {
    [HttpPost("register")]
    public async Task<ActionResult<TokensRes>> Register(RegisterUserReq userReq) {
        var user = new User {
            Email = userReq.Email,
            UserName = userReq.UserName
        };
        var result = await userManager.CreateAsync(user, userReq.Password);
        if (!result.Succeeded)
            return BadRequest(result);

        logger.LogInformation("User created a new account with password.");
        if (userManager.Options.SignIn.RequireConfirmedAccount) {
            return Ok(new {
                Message = "Registration successful. Please check your email to confirm your account."
            });
        }

        var tokens = await GenerateTokensAsync(user);
        return Ok(tokens);
    }

    private async Task<TokensRes> GenerateTokensAsync(User user) {
        var userId = await userManager.GetUserIdAsync(user);
        var sessionId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var refreshToken = tokenManager.GenerateRefreshToken(sessionId.ToString(), now);
        var jwtToken = await tokenManager.GenerateJwtAsync(userId, sessionId.ToString(), now);

        var session = new Session {
            Id = sessionId,
            RefreshTokenExpiresAt = refreshToken.Expire,
            UserId = userId,
            CreatedAt = now,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()!,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };
        await sessionManager.CreateAsync(session, refreshToken.Value);
        await unitOfWork.SaveChangesAsync();

        var tokens = new TokensRes(
            jwtToken.Value,
            jwtToken.Expire,
            refreshToken.Value,
            session.RefreshTokenExpiresAt
        );
        return tokens;
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
            var user = await userManager.GetByEmailAsync(req.Email);
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
        var inputRefreshToken = tokenManager.ParseRefreshToken(req.RefreshToken);
        if (inputRefreshToken is null) return Unauthorized();

        var sessionId = new Guid(inputRefreshToken.Sid);
        var session = await sessionManager.GetByIdAsync(sessionId);
        if (session is null) return Unauthorized();

        var valid = await sessionManager.ValidateRefreshTokenAsync(sessionId, req.RefreshToken);
        if (!valid) return Unauthorized();

        var tokens =
            await tokenManager.GenerateTokensAsync(session.UserId, inputRefreshToken.Sid, DateTime.UtcNow);

        sessionManager.SetRefreshToken(session, tokens.refresh.Value);
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
        var sessionId = HttpContext.User.FindFirstValue(JwtClaims.SessionId);
        if (userId is null || sessionId is null)
            return Unauthorized();
        var sessionGuid = new Guid(sessionId);

        var user = await userManager.GetByIdAsync(userId, false);
        if (user is null) return Unauthorized();

        await unitOfWork.BeginTransactionAsync();

        var result = await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded) return BadRequest(result);

        await sessionManager.RevokeAllExceptAsync(userId, sessionGuid);

        var (jwt, refresh) = await tokenManager.GenerateTokensAsync(userId, sessionId, DateTime.UtcNow);
        _ = await sessionManager.UpdateRefreshTokenAsync(sessionGuid, refresh.Value, refresh.Expire);

        await unitOfWork.CommitTransactionAsync();

        return Ok(new TokensRes(
            jwt.Value,
            jwt.Expire,
            refresh.Value,
            refresh.Expire
        ));
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout() {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = HttpContext.User.FindFirstValue(JwtClaims.SessionId);
        if (userId is null || sessionId is null)
            return Unauthorized();

        var session = await sessionManager.GetByIdAsync(new Guid(sessionId), userId);
        if (session is null) return Unauthorized();

        await sessionManager.RemoveAsync(new Guid(sessionId), userId);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}