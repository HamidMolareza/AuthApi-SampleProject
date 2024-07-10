using AuthApi.Data;
using AuthApi.Helpers;
using AuthApi.Helpers.Store;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth.Services.Role;

public class RoleStore(RoleStore<Entities.Role> roleStore, AppDbContext dbContext)
    : Store<Entities.Role, AppDbContext>(dbContext), IRoleStore {
    private readonly RoleStore<Entities.Role> _roleStore = roleStore;

    public Task<List<Entities.Role>> GetAllAsync(bool asNoTracking, bool includeUsers, bool includeClaims) {
        var query = DbSet.AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.ToListAsync();
    }

    public Task<Entities.Role?> GetByIdAsync(string roleId, bool asNoTracking, bool includeUsers, bool includeClaims) {
        var query = DbSet.Where(role => role.Id == roleId).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.FirstOrDefaultAsync();
    }

    public Task<Entities.Role?> GetByNormalizeNameAsync(string normalizedName, bool asNoTracking, bool includeUsers,
        bool includeClaims) {
        var query = DbSet.Where(role => role.NormalizedName == normalizedName).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.FirstOrDefaultAsync();
    }

    public Task<List<Entities.Role>> GetByNamesAsync(IEnumerable<string> names, bool asNoTracking, bool includeUsers,
        bool includeClaims) {
        var query = DbSet.Where(role => names.Contains(role.Name!.ToLower())).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);

        return query.ToListAsync();
    }

    public Task<List<Entities.Role>> GetByNormalizedNamesAsync(IEnumerable<string> names, bool asNoTracking,
        bool includeUsers,
        bool includeClaims) {
        var query = DbSet.Where(role =>
            names.Contains(role.NormalizedName)).AsQueryable();

        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);

        return query.ToListAsync();
    }

    protected static IQueryable<Entities.Role> AddToQuery(
        IQueryable<Entities.Role> query,
        bool includeUsers,
        bool includeClaims,
        bool asNoTracking) {
        if (asNoTracking) query = query.AsNoTracking();
        if (includeUsers) {
            query = query.Include(role => role.UserRoles)
                .ThenInclude(userRole => userRole.User);
        }

        if (includeClaims) {
            query = query.Include(role => role.RoleClaims);
        }

        return query;
    }
}