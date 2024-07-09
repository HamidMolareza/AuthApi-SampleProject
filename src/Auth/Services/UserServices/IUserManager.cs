using AuthApi.Auth.Entities;
using OnRails;

namespace AuthApi.Auth.Services.UserServices;

public interface IUserManager {
    Task<List<User>> GetAllAsync(bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(string id, bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default);

    Task<Result> AddRolesAsync(string userId, string[] roles, CancellationToken cancellationToken);
    Task<Result> RemoveRolesAsync(string userId, string[] roles, CancellationToken ct);
    Task<int> RemoveAllUsersExceptRolesAsync(params string[] roles);
    Task<int> DeleteByIdAsync(string userId);
}