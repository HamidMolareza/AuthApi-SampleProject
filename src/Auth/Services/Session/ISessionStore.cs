using AuthApi.Helpers;

namespace AuthApi.Auth.Services.Session;

public interface ISessionStore : IStore<Entities.Session> {
    Task<Entities.Session?> GetByIdsAsync(Guid sessionId, string userId, bool asNoTracking, bool includeUser = false,
        CancellationToken cancellationToken = default);

    Task<List<Entities.Session>> GetAllAsync(bool asNoTracking, string? userId = null, bool includeUser = false,
        CancellationToken cancellationToken = default);

    Task RevokeAllExceptAsync(string userId, Guid sessionId);
    Task DeleteAsync(Guid sessionId, string userId);
}