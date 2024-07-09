using AuthApi.Data;
using AuthApi.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth.Services.Session;

public class SessionManager(AppDbContext db) : ISessionManager {
    public Task<Entities.Session?> GetByIdAsync(Guid id) {
        return db.Sessions.FirstOrDefaultAsync(session => session.Id == id);
    }

    public Task<Entities.Session?> GetByIdAsync(Guid id, string userId) {
        return db.Sessions
            .Where(s => s.UserId == userId)
            .FirstOrDefaultAsync(session => session.Id == id);
    }

    public async Task CreateAsync(Entities.Session session, string refreshTokenValue) {
        SetRefreshToken(session, refreshTokenValue);
        await db.Sessions.AddAsync(session);
    }

    public async Task RevokeAllExceptAsync(string userId, Guid sessionId) {
        var sessions = await db.Sessions.Where(session => session.UserId == userId && session.Id != sessionId)
            .ToListAsync();
        db.Sessions.RemoveRange(sessions);
    }

    public async Task<Entities.Session?> UpdateRefreshTokenAsync(Guid sessionId, string refreshToken,
        DateTime refreshExpire) {
        var session = await db.Sessions.FindAsync(sessionId);
        if (session is null) return session;

        SetRefreshToken(session, refreshToken);
        session.RefreshTokenExpiresAt = refreshExpire.ToUniversalTime();

        return session;

        //TODO: concurrency
    }

    public Task<List<Entities.Session>> GetAllAsync(string? userId = null) {
        var query = db.Sessions.AsQueryable();
        if (userId is not null)
            query = query.Where(s => s.UserId == userId);
        return query.ToListAsync();
    }

    public async Task RemoveAsync(Guid sessionId, string userId) {
        var sessions = await db.Sessions.Where(session => session.UserId == userId && session.Id == sessionId)
            .ToListAsync();
        db.Sessions.RemoveRange(sessions);
    }

    public void SetRefreshToken(Entities.Session session, string refreshTokenValue) {
        session.RefreshTokenHash = SecurityHelpers.Sha512(refreshTokenValue);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid sessionId, string refreshTokenValue) {
        var session = await db.Sessions.FindAsync(sessionId);
        if (session is null) return false;

        return SecurityHelpers.Sha512(refreshTokenValue) == session.RefreshTokenHash;
    }
}