using AuthApi.Auth.Entities;

namespace AuthApi.Auth.Services;

public interface ISessionManager {
    public Task<Session?> GetByIdAsync(Guid id);
    Task<Session?> GetByIdAsync(Guid id, string userId);
    public Task CreateAsync(Session session);
    public Task RevokeAllExceptAsync(Guid sessionId, string userId);
    public Task UpdateRefreshTokenAsync(Guid sessionId, string refreshToken, DateTime refreshExpire);
    public Task<List<Session>> GetAllAsync(string? userId = null);
    public Task RemoveAsync(Guid sessionId, string userId);
}