using AuthApi.Auth.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<User> userManager,
    IAuthService authService,
    ILogger<AuthController> logger) : ControllerBase {
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterUserReq userReq) {
        var user = new User {
            Email = userReq.Email,
            UserName = userReq.UserName
        };

        var result = await userManager.CreateAsync(user, userReq.Password);
        if (!result.Succeeded) {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
            return BadRequest(ModelState);
        }

        logger.LogInformation("User created a new account with password.");
        if (userManager.Options.SignIn.RequireConfirmedAccount) {
            return Ok(new {
                Message = "Registration successful. Please check your email to confirm your account."
            });
        }

        var token = await authService.CreateTokenAsync(user);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginReq req) {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null) return NotFound();

        var isPasswordValid = await userManager.CheckPasswordAsync(user, req.Password);
        if (!isPasswordValid) return Unauthorized();

        var token = await authService.CreateTokenAsync(user);
        return Ok(new { token });
    }
}