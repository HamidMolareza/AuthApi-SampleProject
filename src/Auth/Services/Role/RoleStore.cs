using AuthApi.Data;
using AuthApi.Helpers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Auth.Services.Role;

public class RoleStore : Store<Entities.Role, AppDbContext>, IRoleStore {
    private readonly RoleStore<Entities.Role> _roleStore;
    private readonly AppDbContext _db;

    public RoleStore(RoleStore<Entities.Role> roleStore, AppDbContext dbContext) : base(dbContext) {
        _roleStore = roleStore;
        _db = dbContext;
    }

    public Task<List<Entities.Role>> GetAllAsync(bool asNoTracking, bool includeUsers, bool includeClaims) {
        var query = _db.Roles.AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.ToListAsync();
    }

    public Task<Entities.Role?> GetByIdAsync(string roleId, bool asNoTracking, bool includeUsers, bool includeClaims) {
        var query = _db.Roles.Where(role => role.Id == roleId).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.FirstOrDefaultAsync();
    }

    public Task<Entities.Role?> GetByNormalizeNameAsync(string normalizedName, bool asNoTracking, bool includeUsers,
        bool includeClaims) {
        var query = _db.Roles.Where(role => role.NormalizedName == normalizedName).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.FirstOrDefaultAsync();
    }

    public Task<List<Entities.Role>> GetByNamesAsync(IEnumerable<string> names, bool asNoTracking, bool includeUsers,
        bool includeClaims) {
        var query = _db.Roles.Where(role => names.Contains(role.Name!.ToLower())).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);

        return query.ToListAsync();
    }

    public Task<List<Entities.Role>> GetByNormalizedNamesAsync(IEnumerable<string> names, bool asNoTracking,
        bool includeUsers,
        bool includeClaims) {
        var query = _db.Roles.Where(role =>
                names.Contains(role.NormalizedName.ToLower()))
            .AsQueryable();

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