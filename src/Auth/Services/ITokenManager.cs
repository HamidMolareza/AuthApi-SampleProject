using AuthApi.Auth.Options;

namespace AuthApi.Auth.Services;

public interface ITokenManager {
    public record RefreshToken(string Token, string Sid); //Sid = SessionId

    public record Token(string Value, DateTime Expire);

    public JwtOptions JwtOptions { get; }
    public RefreshTokenOptions RefreshTokenOptions { get; }

    public Task<Token> GenerateJwtAsync(string userId, string sessionId, DateTime currentDateTimeUtc,
        bool addRoleClaims = true);

    public Token GenerateRefreshToken(string sessionId, DateTime currentDateTimeUtc);

    public Task<(Token jwt, Token refresh)> GenerateTokensAsync(string userId, string sessionId,
        DateTime currentDateTimeUtc,
        bool addRoleClaims = true);

    public RefreshToken? ParseRefreshToken(string token);
}