using AuthApi.Auth.Entities;

namespace AuthApi.Auth.Services;

public interface ISessionManager {
    public Task<Session?> GetByIdAsync(Guid id);
    public Task CreateAsync(Session session);
    public Task RevokeAllExceptAsync(Guid sessionId, string userId);
    public Task UpdateRefreshTokenAsync(Guid sessionId, string refreshToken, DateTime refreshExpire);
}