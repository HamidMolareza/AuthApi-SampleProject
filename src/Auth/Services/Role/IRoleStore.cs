using AuthApi.Helpers;

namespace AuthApi.Auth.Services.Role;

public interface IRoleStore : IStore<Entities.Role> {
    Task<List<Entities.Role>> GetAllAsync(bool asNoTracking, bool includeUsers, bool includeClaims);

    Task<Entities.Role?> GetByIdAsync(string roleId,
        bool asNoTracking, bool includeUsers, bool includeClaims);

    Task<Entities.Role?> GetByNormalizeNameAsync(string normalizedName, bool asNoTracking, bool includeUsers,
        bool includeClaims);

    public Task<List<Entities.Role>> GetByNamesAsync(IEnumerable<string> names, bool asNoTracking, bool includeUsers,
        bool includeClaims);

    Task<List<Entities.Role>> GetByNormalizedNamesAsync(IEnumerable<string> names, bool asNoTracking, bool includeUsers,
        bool includeClaims);
}