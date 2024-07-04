using System.Security.Claims;
using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Program;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth;

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

        var tokens = await unitOfWork.TokenManager.GenerateTokensAsync(user, DateTime.UtcNow);
        return Ok(new TokensRes(
            tokens.jwt.Value,
            tokens.jwt.Expire,
            tokens.refresh.Value,
            tokens.refresh.Expire
        ));
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
            if (user is null)
                return Unauthorized(new { Message = "Invalid login attempt." });
            var tokens = await unitOfWork.TokenManager.GenerateTokensAsync(user, DateTime.UtcNow);
            return Ok(new TokensRes(
                tokens.jwt.Value,
                tokens.jwt.Expire,
                tokens.refresh.Value,
                tokens.refresh.Expire
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
        var user = await unitOfWork.UserManager.FindByIdAsync(req.UserId);
        if (user is null) return Unauthorized();

        if (user.RefreshToken != req.RefreshToken) return Unauthorized();
        if (user.RefreshTokenExpireTime <= DateTime.UtcNow) return Unauthorized();

        var tokens = await unitOfWork.TokenManager.GenerateTokensAsync(user, DateTime.UtcNow);
        return Ok(new TokensRes(
            tokens.jwt.Value,
            tokens.jwt.Expire,
            tokens.refresh.Value,
            tokens.refresh.Expire
        ));
    }
}