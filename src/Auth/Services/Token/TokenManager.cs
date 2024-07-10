using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Options;
using AuthApi.Auth.Services.UserServices;
using AuthApi.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Auth.Services.Token;

public class TokenManager(
    IOptions<JwtOptions> jwtOptions,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IUserManager userManager) : ITokenManager {
    public JwtOptions JwtOptions { get; } = jwtOptions.Value;
    public RefreshTokenOptions RefreshTokenOptions { get; } = refreshTokenOptions.Value;

    public async Task<ITokenManager.Token> GenerateJwtAsync(string userId, string sessionId,
        DateTime currentDateTimeUtc,
        bool addRoleClaims = true) {
        var claims = await GetClaimsAsync(userId, sessionId, addRoleClaims);
        var jwtToken = CreateJwtToken(claims, currentDateTimeUtc.ToUniversalTime());
        return jwtToken;
    }

    public ITokenManager.Token GenerateRefreshToken(string sessionId, DateTime currentDateTimeUtc) {
        var refreshTokenData =
            new ITokenManager.RefreshToken(SecurityHelpers.GenerateSecureRandomBase64(RefreshTokenOptions.Length),
                sessionId);
        var token = JsonSerializer.Serialize(refreshTokenData).ConvertToBase64();

        var expire = currentDateTimeUtc.ToUniversalTime().AddHours(RefreshTokenOptions.ExpiresInHours);

        return new ITokenManager.Token(token, expire);
    }

    public async Task<(ITokenManager.Token jwt, ITokenManager.Token refresh)> GenerateTokensAsync(string userId,
        string sessionId,
        DateTime currentDateTimeUtc,
        bool addRoleClaims = true) {
        var jwt = await GenerateJwtAsync(userId, sessionId, currentDateTimeUtc, addRoleClaims);
        var refreshToken = GenerateRefreshToken(sessionId, currentDateTimeUtc);

        return new ValueTuple<ITokenManager.Token, ITokenManager.Token>(jwt, refreshToken);
    }

    public ITokenManager.RefreshToken? ParseRefreshToken(string token) {
        var jsonStr = token.ConvertFromBase64();
        return JsonSerializer.Deserialize<ITokenManager.RefreshToken>(jsonStr);
    }

    private ITokenManager.Token CreateJwtToken(IEnumerable<Claim> claims, DateTime currentDateTimeUtc) {
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
        return new ITokenManager.Token(jwtToken, expire);
    }

    private async Task<List<Claim>> GetClaimsAsync(string userId, string sessionId, bool addRoleClaims = true) {
        // Avoid sensitive information (e.g., passwords) and large or non-essential data to maintain security and efficiency.

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtClaims.SessionId, sessionId),
        };

        if (addRoleClaims) {
            var roles = await userManager.GetRolesAsync(new User { Id = userId });
            claims.AddRange(roles.Select(role =>
                new Claim(ClaimTypes.Role, role))
            );
        }

        return claims;
    }
}