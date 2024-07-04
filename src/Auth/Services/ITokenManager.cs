using AuthApi.Auth.Dto;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Options;

namespace AuthApi.Auth.Services;

public interface ITokenManager {
    public JwtOptions JwtOptions { get; }
    public RefreshTokenOptions RefreshTokenOptions { get; }
    public Task<Token> GenerateJwtAsync(User user, DateTime currentDateTimeUtc, bool addRoleClaims = true);
    public Task<Token> GenerateRefreshTokenAsync(User user, DateTime currentDateTimeUtc);

    public Task<(Token jwt, Token refresh)> GenerateTokensAsync(User user, DateTime currentDateTimeUtc,
        bool addRoleClaims = true);
}