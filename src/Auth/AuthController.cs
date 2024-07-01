using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Auth;

[ApiController]
[Route("[controller]")]
public class AuthController(IMapper mapper, UserManager<User> userManager) : ControllerBase { }