namespace AuthApi.Auth.Services.Session;

public interface ISessionManager {
    public Task<Entities.Session?> GetByIdAsync(Guid id);
    Task<Entities.Session?> GetByIdAsync(Guid id, string userId);
    public Task CreateAsync(Entities.Session session, string refreshTokenValue);
    public Task RevokeAllExceptAsync(string userId, Guid sessionId);
    public Task<Entities.Session?> UpdateRefreshTokenAsync(Guid sessionId, string refreshToken, DateTime refreshExpire);
    public Task<List<Entities.Session>> GetAllAsync(string? userId = null);
    public Task RemoveAsync(Guid sessionId, string userId);
    public void SetRefreshToken(Entities.Session session, string refreshTokenValue);
    public Task<bool> ValidateRefreshTokenAsync(Guid sessionId, string refreshTokenValue);
}