using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Options;
using AuthApi.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Auth.Services;

public class TokenManager(
    IOptions<JwtOptions> jwtOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    UserManager userManager
) : ITokenManager {
    public JwtOptions JwtOptions { get; } = jwtOptions.Value;
    public RefreshTokenOptions RefreshTokenOptions { get; } = refreshTokenOptions.Value;

    public async Task<Token> GenerateJwtAsync(User user, DateTime currentDateTimeUtc, bool addRoleClaims = true) {
        var claims = await GetClaimsAsync(user, addRoleClaims);
        var jwtToken = CreateJwtToken(claims, currentDateTimeUtc.ToUniversalTime());
        return jwtToken;
    }

    public async Task<Token> GenerateRefreshTokenAsync(User user, DateTime currentDateTimeUtc) {
        var refreshToken = SecurityHelpers.GenerateSecureRandomBase64(RefreshTokenOptions.Length);
        var expire = currentDateTimeUtc.ToUniversalTime().AddHours(RefreshTokenOptions.ExpiresInHours);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpireTime = expire;

        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
            return new Token(refreshToken, expire);

        var errors = result.Errors.Select(error => $"{error.Code}: {error.Description}");
        throw new Exception(string.Join('\n', errors));
    }

    public async Task<(Token jwt, Token refresh)> GenerateTokensAsync(User user, DateTime currentDateTimeUtc,
        bool addRoleClaims = true) {
        var jwt = await GenerateJwtAsync(user, currentDateTimeUtc, addRoleClaims);
        var refreshToken = await GenerateRefreshTokenAsync(user, currentDateTimeUtc);

        return new ValueTuple<Token, Token>(jwt, refreshToken);
    }

    private Token CreateJwtToken(IEnumerable<Claim> claims, DateTime currentDateTimeUtc) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(JwtOptions.SecretKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var expire = currentDateTimeUtc.ToUniversalTime().AddSeconds(JwtOptions.ExpiresInSeconds);
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = expire,
            SigningCredentials = credentials,
            Issuer = JwtOptions.Issuer,
            Audience = JwtOptions.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        return new Token(jwtToken, expire);
    }

    private async Task<List<Claim>> GetClaimsAsync(User user, bool addRoleClaims = true) {
        //Avoid sensitive information (e.g., passwords) and large or non-essential data to maintain security and efficiency.

        var securityStamp = user.SecurityStamp ??
                            throw new Exception(
                                $"Can not set {nameof(user.SecurityStamp)} in JWT claims because it is null.");

        //TODO: Add another?
        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(Claims.SecurityStamp, securityStamp),
        };

        if (addRoleClaims) {
            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role =>
                new Claim(ClaimTypes.Role, role))
            );
        }

        return claims;
    }
}