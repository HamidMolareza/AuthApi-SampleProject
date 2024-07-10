using AuthApi.Auth.Entities;
using AuthApi.Data;
using AuthApi.Helpers;
using AuthApi.Helpers.Store;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth.Services.UserServices;

public class UserStore(AppDbContext dbContext, IUserStore<User> aspStore)
    : Store<User, AppDbContext>(dbContext), IUserStore {
    public Task<List<User>> GetAllAsync(bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default) {
        var query = DbSet.AsQueryable();
        query = AddToQuery(query, asNoTracking, includeRoles, includeClaims, includeLogins, includeTokens,
            includeSessions);
        return query.ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(string id, bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default) {
        var query = DbSet.Where(item => item.Id == id).AsQueryable();
        query = AddToQuery(query, asNoTracking, includeRoles, includeClaims, includeLogins, includeTokens,
            includeSessions);
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public void RemoveAllUsersExceptRoles(IEnumerable<string> roles) {
        var query = DbSet.AsQueryable();
        query = AddToQuery(query, false, true, false, false, false,
            false);
        query = query.Where(u => u.UserRoles.All(ur => !roles.Contains(ur.Role.Name)));

        DbSet.RemoveRange(query);
    }

    protected static IQueryable<User> AddToQuery(
        IQueryable<User> query,
        bool asNoTracking,
        bool includeRoles = false,
        bool includeClaims = false,
        bool includeLogins = false,
        bool includeTokens = false,
        bool includeSessions = false) {
        if (asNoTracking) query = query.AsNoTracking();
        if (includeRoles)
            query = query.Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role);
        if (includeClaims)
            query = query.Include(user => user.UserClaims);
        if (includeLogins)
            query = query.Include(user => user.Logins);
        if (includeTokens)
            query = query.Include(user => user.Tokens);
        if (includeSessions)
            query = query.Include(user => user.Sessions);

        return query;
    }
}