using AuthApi.Auth.Entities;
using AuthApi.Data;
using AuthApi.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AuthApi.Auth.Services;

public class RoleManager(
    AppDbContext db,
    IRoleStore<Role> store,
    IEnumerable<IRoleValidator<Role>> roleValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    ILogger<RoleManager<Role>> logger)
    : RoleManager<Role>(store, roleValidators, keyNormalizer, errors, logger) {
    public Task<List<Role>> GetAllAsync(bool includeUsers, bool includeClaims, bool asNoTracking) {
        var query = db.Roles.AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.ToListAsync();
    }

    private static IQueryable<Role> AddToQuery(IQueryable<Role> query, bool includeUsers, bool includeClaims,
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

    public Task<Role?> GetByIdAsync(string roleId, bool includeUsers, bool includeClaims, bool asNoTracking) {
        var query = db.Roles.Where(role => role.Id == roleId).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.FirstOrDefaultAsync();
    }

    public Task<Role?> GetByNameAsync(string roleName, bool includeUsers, bool includeClaims, bool asNoTracking) {
        roleName = roleName.ToLower();
        var query = db.Roles.Where(role => role.Name!.ToLower() == roleName).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.FirstOrDefaultAsync();
    }

    public Task<List<Role>> GetByNamesAsync(IEnumerable<string> roleNames, bool includeUsers, bool includeClaims,
        bool asNoTracking) {
        var uniqueLowerCaseNames = roleNames.GetUniqueLowerCase();
        var query = db.Roles.Where(role => uniqueLowerCaseNames.Contains(role.Name!.ToLower())).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.ToListAsync();
    }

    public Task<List<Role>> GetByExcludeNamesAsync(IEnumerable<string> roleNames, bool includeUsers, bool includeClaims,
        bool asNoTracking) {
        var uniqueLowerCaseNames = roleNames.GetUniqueLowerCase();
        var query = db.Roles.Where(role => !uniqueLowerCaseNames.Contains(role.Name!.ToLower())).AsQueryable();
        query = AddToQuery(query, includeUsers, includeClaims, asNoTracking);
        return query.ToListAsync();
    }

    public async Task<List<string>> GetNewRoleNamesAsync(List<string> names) {
        var uniqueLowerCaseNames = names.GetUniqueLowerCase();
        var existNames = await db.Roles
            .Where(role => !string.IsNullOrEmpty(role.Name))
            .Where(role => uniqueLowerCaseNames.Contains(role.Name!.ToLower()))
            .Select(role => role.Name).ToListAsync();

        return names.Except(existNames, new CaseInsensitiveValueComparer()).ToList()!;
    }

    public async Task<List<Role>> AddNewNamesAsync(List<string> names) {
        var newRoleNames = await GetNewRoleNamesAsync(names);

        var newRoles = newRoleNames.Select(name => new Role(name)).ToList();
        await db.Roles.AddRangeAsync(newRoles);

        return newRoles;
    }

    public void RemoveRanges(IEnumerable<Role> roles) {
        db.Roles.RemoveRange(roles);
    }

    public Task AddRangesAsync(IEnumerable<Role> roles) {
        return db.Roles.AddRangeAsync(roles);
    }
}