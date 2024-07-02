using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Auth.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Auth;

public class AuthService(IOptions<JwtOptions> jwtOptions, UserManager<User> userManager) : IAuthService {
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<string> CreateTokenAsync(User user) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtOptions.SecretKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);
        var claims = await GetClaimsAsync(user);

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(_jwtOptions.ExpiresInSeconds),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<List<Claim>> GetClaimsAsync(User user) {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id) };
        if (user.Email is not null && user.EmailConfirmed)
            claims.Add(new Claim(ClaimTypes.Name, user.Email));

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }
}