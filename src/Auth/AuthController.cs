using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    SignInManager<User> signInManager,
    ILogger<AuthController> logger) : ControllerBase {
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterUserReq userReq) {
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

        var token = await unitOfWork.CreateJwtTokenAsync(user);
        return Ok(new { token });
    }

    private ActionResult BadRequest(IdentityResult result) {
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginReq req) {
        var result = await signInManager.PasswordSignInAsync(req.Email,
            req.Password, false, lockoutOnFailure: true);

        if (result.Succeeded) {
            var user = await userManager.FindByEmailAsync(req.Email);
            if (user is null)
                return Unauthorized(new { Message = "Invalid login attempt." });
            var token = await unitOfWork.CreateJwtTokenAsync(user);
            return Ok(new { token });
        }

        if (result.RequiresTwoFactor) {
            return Unauthorized(new { Message = "RequiresTwoFactor" });
        }

        if (result.IsLockedOut) {
            return Forbid();
        }

        return Unauthorized(new { Message = "Invalid login attempt." });
    }
}