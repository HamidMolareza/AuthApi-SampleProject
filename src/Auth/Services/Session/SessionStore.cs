using AuthApi.Data;
using AuthApi.Helpers;
using AuthApi.Helpers.Store;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth.Services.Session;

public class SessionStore(AppDbContext db) : Store<Entities.Session, AppDbContext>(db), ISessionStore {
    public Task<Entities.Session?>
        GetByIdsAsync(Guid sessionId, string userId, bool asNoTracking, bool includeUser,
            CancellationToken cancellationToken = default) {
        var query = DbSet.Where(item => item.Id == sessionId && item.UserId == userId);
        query = AddToQuery(query, asNoTracking, includeUser);
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<Entities.Session>> GetAllAsync(bool asNoTracking, string? userId = null, bool includeUser = false,
        CancellationToken cancellationToken = default) {
        var query = DbSet.AsQueryable();
        query = AddToQuery(query, asNoTracking, includeUser);
        if (userId is not null)
            query = query.Where(s => s.UserId == userId);
        return query.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task RevokeAllExceptAsync(string userId, Guid sessionId) {
        var sessions = await DbSet.Where(session => session.UserId == userId && session.Id != sessionId)
            .ToListAsync();
        DbSet.RemoveRange(sessions);
    }

    public async Task DeleteAsync(Guid sessionId, string userId) {
        var sessions = await DbSet.Where(session => session.UserId == userId && session.Id == sessionId)
            .ToListAsync();
        DbSet.RemoveRange(sessions);
    }

    protected static IQueryable<Entities.Session> AddToQuery(
        IQueryable<Entities.Session> query,
        bool asNoTracking,
        bool includeUser) {
        if (asNoTracking) query = query.AsNoTracking();
        if (includeUser)
            query = query.Include(item => item.User);

        return query;
    }
}