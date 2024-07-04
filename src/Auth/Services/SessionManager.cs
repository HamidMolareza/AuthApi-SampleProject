using AuthApi.Auth.Entities;
using AuthApi.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth.Services;

public class SessionManager(AppDbContext db) : ISessionManager {
    public Task<Session?> GetByIdAsync(Guid id) {
        return db.Sessions.FirstOrDefaultAsync(session => session.Id == id);
    }

    public async Task CreateAsync(Session session) {
        await db.Sessions.AddAsync(session);
    }

    public async Task RevokeAllExceptAsync(Guid sessionId, string userId) {
        var sessions = await db.Sessions.Where(session => session.UserId == userId && session.Id != sessionId)
            .ToListAsync();
        db.Sessions.RemoveRange(sessions);
    }

    public async Task UpdateRefreshTokenAsync(Guid sessionId, string refreshToken, DateTime refreshExpire) {
        var session = await db.Sessions.FindAsync(sessionId);
        if (session is null) throw new Exception($"Can not find any session with id '{sessionId}'.");

        session.RefreshToken = refreshToken;
        session.RefreshTokenExpiresAt = refreshExpire.ToUniversalTime();

        //TODO: concurrency
    }
}