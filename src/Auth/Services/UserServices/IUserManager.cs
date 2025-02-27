using AuthApi.Auth.Entities;
using AuthApi.Helpers.Manager;
using Microsoft.AspNetCore.Identity;
using OnRails;

namespace AuthApi.Auth.Services.UserServices;

public interface IUserManager : IManager<User> {
    public IdentityOptions Options { get; set; }

    Task<List<User>> GetAllAsync(bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(string id, bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default);

    Task<string> GetUserIdAsync(User user);
    Task<User?> GetByEmailAsync(string email);

    Task<Result> AddRolesAsync(string userId, string[] roles, CancellationToken cancellationToken = default);

    Task<Result> DeleteRolesAsync(string userId, string[] roles, CancellationToken ct);
    Task<int> DeleteAllUsersExceptRolesAsync(params string[] roles);
    Task<int> DeleteByIdAsync(string userId);

    Task<IdentityResult> CreateAsync(User user, string password);
    Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<IList<string>> GetRolesAsync(User user);
}