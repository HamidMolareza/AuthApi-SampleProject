using AuthApi.Helpers.Manager;

namespace AuthApi.Auth.Services.Session;

public interface ISessionManager : IManager<Entities.Session> {
    Task<List<Entities.Session>> GetAllAsync(bool asNoTracking, string? userId = null, bool includeUser = false,
        CancellationToken cancellationToken = default);

    ValueTask<Entities.Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Entities.Session?> GetByIdAsync(Guid sessionId, string userId, CancellationToken cancellationToken = default);

    Task CreateAsync(Entities.Session session, string refreshTokenValue);

    Task<Entities.Session?> UpdateRefreshTokenAsync(Guid sessionId, string refreshToken, DateTime refreshExpire);
    void SetRefreshToken(Entities.Session session, string refreshTokenValue);

    Task RevokeAllExceptAsync(string userId, Guid sessionId);
    Task RemoveAsync(Guid sessionId, string userId);

    Task<bool> ValidateRefreshTokenAsync(Guid sessionId, string refreshTokenValue,
        CancellationToken cancellationToken = default);
}