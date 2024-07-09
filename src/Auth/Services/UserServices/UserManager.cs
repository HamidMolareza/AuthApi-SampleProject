using AuthApi.Auth.Entities;
using AuthApi.Helpers;
using AuthApi.Program;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using OnRails;
using OnRails.ResultDetails.Errors;

namespace AuthApi.Auth.Services.UserServices;

public class UserManager(
    IUnitOfWork unitOfWork,
    UserManager<User> aspUserManager,
    IOptions<IdentityOptions> optionsAccessor) : IUserManager {
    public IdentityOptions Options { get; set; } = optionsAccessor.Value;

    public Task<List<User>> GetAllAsync(bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default) {
        return unitOfWork.UserStore.GetAllAsync(asNoTracking, includeRoles, includeClaims, includeLogins, includeTokens,
            includeSessions, cancellationToken);
    }

    public Task<User?> GetByIdAsync(string id, bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default) {
        return unitOfWork.UserStore.GetByIdAsync(id, asNoTracking, includeRoles, includeClaims, includeLogins,
            includeTokens,
            includeSessions, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email) {
        return aspUserManager.FindByEmailAsync(email);
    }

    public async Task<Result> AddRolesAsync(string userId, string[] roles, CancellationToken cancellationToken) {
        if (roles.Length == 0) return Result.Ok();

        var user = await GetByIdAsync(userId, false, includeRoles: true, cancellationToken: cancellationToken);
        if (user is null) return Result.Fail(new NotFoundError(nameof(userId), userId, view: true));

        var newRoles = roles
            .Except(user.UserRoles.Select(ur => ur.Role.NormalizedName),
                new CaseInsensitiveValueComparer())
            .ToList();
        if (newRoles.Count == 0) return Result.Ok();

        var identityResult = await aspUserManager.AddToRolesAsync(user, newRoles);
        return identityResult.MapToResult();
    }

    public async Task<Result> DeleteRolesAsync(string userId, string[] roles, CancellationToken ct) {
        if (roles.Length == 0) return Result.Ok();

        var user = await GetByIdAsync(userId, false, includeRoles: true, cancellationToken: ct);
        if (user is null) return Result.Fail(new NotFoundError(nameof(userId), userId));

        var existRoles = roles.Select(role => role.ToLower())
            .Where(role => user.UserRoles.Any(ur => ur.Role.Name!.ToLower() == role))
            .ToList();
        if (existRoles.Count == 0) return Result.Ok();

        var identityResult = await aspUserManager.RemoveFromRolesAsync(user, existRoles);
        return identityResult.MapToResult();
    }

    public Task<int> DeleteAllUsersExceptRolesAsync(params string[] roles) {
        unitOfWork.UserStore.RemoveAllUsersExceptRoles(roles);
        return unitOfWork.SaveChangesAsync();
    }

    public async Task<int> DeleteByIdAsync(string userId) {
        var user = await unitOfWork.UserStore.GetByIdAsync(userId, false);
        if (user is null) return 0;

        unitOfWork.UserStore.Delete(user);
        return await unitOfWork.SaveChangesAsync();
    }

    public Task<IdentityResult> CreateAsync(User user, string password) {
        return aspUserManager.CreateAsync(user, password);
    }

    public Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword) {
        return aspUserManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public Task<string> GetUserIdAsync(User user) {
        return aspUserManager.GetUserIdAsync(user);
    }
}