using AuthApi.Helpers;
using AuthApi.Helpers.Manager;
using AuthApi.Program;

namespace AuthApi.Auth.Services.Session;

public class SessionManager(IUnitOfWork unitOfWork)
    : Manager<ISessionStore, Entities.Session>(unitOfWork.SessionStore), ISessionManager {
    public ValueTask<Entities.Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default) {
        return unitOfWork.SessionStore.GetByIdsAsync([sessionId], true, cancellationToken: cancellationToken);
    }

    public Task<Entities.Session?> GetByIdAsync(Guid sessionId, string userId,
        CancellationToken cancellationToken = default) {
        return unitOfWork.SessionStore.GetByIdsAsync(sessionId, userId, true, cancellationToken: cancellationToken);
    }

    public Task CreateAsync(Entities.Session session, string refreshTokenValue) {
        SetRefreshToken(session, refreshTokenValue);
        return unitOfWork.SessionStore.AddAsync(session);
    }

    public Task RevokeAllExceptAsync(string userId, Guid sessionId) {
        return unitOfWork.SessionStore.RevokeAllExceptAsync(userId, sessionId);
    }

    public async Task<Entities.Session?> UpdateRefreshTokenAsync(Guid sessionId, string refreshToken,
        DateTime refreshExpire) {
        var session = await unitOfWork.SessionStore.GetByIdsAsync([sessionId], false);
        if (session is null) return session;

        SetRefreshToken(session, refreshToken);
        session.RefreshTokenExpiresAt = refreshExpire.ToUniversalTime();

        return session;
    }

    public Task<List<Entities.Session>> GetAllAsync(bool asNoTracking, string? userId = null, bool includeUser = false,
        CancellationToken cancellationToken = default) {
        return unitOfWork.SessionStore.GetAllAsync(asNoTracking, userId, includeUser, cancellationToken);
    }

    public Task RemoveAsync(Guid sessionId, string userId) {
        return unitOfWork.SessionStore.DeleteAsync(sessionId, userId);
    }

    public void SetRefreshToken(Entities.Session session, string refreshTokenValue) {
        session.RefreshTokenHash = SecurityHelpers.Sha512(refreshTokenValue);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid sessionId, string refreshTokenValue,
        CancellationToken cancellationToken = default) {
        var session = await unitOfWork.SessionStore.GetByIdsAsync([sessionId], false, cancellationToken);
        if (session is null) return false;

        return SecurityHelpers.Sha512(refreshTokenValue) == session.RefreshTokenHash;
    }
}