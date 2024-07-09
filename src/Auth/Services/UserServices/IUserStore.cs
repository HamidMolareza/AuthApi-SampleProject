using AuthApi.Auth.Entities;
using AuthApi.Helpers;

namespace AuthApi.Auth.Services.UserServices;

public interface IUserStore : IStore<User> {
    Task<List<User>> GetAllAsync(bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(string id, bool asNoTracking, bool includeRoles = false, bool includeClaims = false,
        bool includeLogins = false, bool includeTokens = false, bool includeSessions = false,
        CancellationToken cancellationToken = default);

    void RemoveAllUsersExceptRoles(IEnumerable<string> roles);
}