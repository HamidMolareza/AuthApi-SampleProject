using AuthApi.Helpers;
using AuthApi.Program;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnRails;
using OnRails.ResultDetails.Errors;
using OnRails.ResultDetails.Success;

namespace AuthApi.Auth.Services.Role;

public class RoleManager(IUnitOfWork unitOfWork, RoleManager<Entities.Role> aspRoleManager) : IRoleManager {
    public async Task<List<Entities.Role>> CreateRolesAsync(List<string> names) {
        var newRoleNames = await GetNewRoleNamesAsync(names);

        var newRoles = newRoleNames.Select(name => new Entities.Role(name)).ToList();
        await unitOfWork.RoleStore.AddRangeAsync(newRoles);
        await unitOfWork.SaveChangesAsync();

        return newRoles;
    }

    public Task<List<Entities.Role>> GetAllAsync(bool asNoTracking, bool includeUsers = false,
        bool includeClaims = false) {
        return unitOfWork.RoleStore.GetAllAsync(asNoTracking, includeUsers, includeClaims);
    }

    public Task<Entities.Role?> GetByIdAsync(string roleId, bool asNoTracking, bool includeUsers = false,
        bool includeClaims = false) {
        return unitOfWork.RoleStore.GetByIdAsync(roleId, asNoTracking, includeUsers, includeClaims);
    }

    public Task<Entities.Role?> GetByNameAsync(string roleName, bool asNoTracking, bool includeUsers = false,
        bool includeClaims = false) {
        var normalizedName = aspRoleManager.NormalizeKey(roleName);
        return unitOfWork.RoleStore.GetByNormalizeNameAsync(normalizedName, asNoTracking, includeUsers, includeClaims);
    }

    public async Task<List<string>> GetNewRoleNamesAsync(List<string> names) {
        var uniqueNormalizedNames = GetUniqueNormalizedNames(names);

        var existNames =
            (await unitOfWork.RoleStore.GetByNormalizedNamesAsync(uniqueNormalizedNames, true, false, false))
            .Select(role => role.Name!)
            .ToList();

        return names.Except(existNames, new CaseInsensitiveValueComparer()).ToList();
    }

    public async Task<Result> UpdateByIdAsync(string id, string newName) {
        var role = await GetByIdAsync(id, false);
        if (role is null) return Result.Fail(new NotFoundError(nameof(id), id, view: true));

        var newRole = await GetByNameAsync(newName, false);
        if (newRole is not null) return Result.Fail(new ConflictError(view: true));

        var identityResult = await aspRoleManager.SetRoleNameAsync(role, newName);
        if (!identityResult.Succeeded)
            return identityResult.MapToResult();

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteByIdAsync(string id) {
        var role = await GetByIdAsync(id, false);
        if (role is null) return Result.Ok(new NoContentDetail());

        unitOfWork.RoleStore.Delete(role);
        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<List<Entities.Role>> DeleteByNamesAsync(IEnumerable<string> names) {
        var uniqueNormalizedNames = GetUniqueNormalizedNames(names);

        var roles = await unitOfWork.RoleStore.GetByNormalizedNamesAsync(
            uniqueNormalizedNames, false, false, false);

        unitOfWork.RoleStore.DeleteRange(roles);
        await unitOfWork.SaveChangesAsync();

        return roles;
    }

    private List<string> GetUniqueNormalizedNames(IEnumerable<string?> names) =>
        names.Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .Select(aspRoleManager.NormalizeKey)
            .ToList()!;
}