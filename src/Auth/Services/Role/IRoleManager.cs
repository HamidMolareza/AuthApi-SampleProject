using AuthApi.Helpers.Manager;
using OnRails;

namespace AuthApi.Auth.Services.Role;

public interface IRoleManager : IManager<Entities.Role> {
    public Task<List<Entities.Role>> CreateRolesAsync(List<string> names);
    Task<List<Entities.Role>> GetAllAsync(bool asNoTracking, bool includeUsers = false, bool includeClaims = false);

    Task<Entities.Role?> GetByIdAsync(string id, bool asNoTracking, bool includeUsers = false,
        bool includeClaims = false);

    Task<Entities.Role?> GetByNameAsync(string roleName, bool asNoTracking, bool includeUsers = false,
        bool includeClaims = false);

    Task<List<string>> GetNewRoleNamesAsync(List<string> names);
    Task<bool> RoleExistsAsync(string roleName);

    Task<Result> UpdateByIdAsync(string id, string newName);
    Task<Result> DeleteByIdAsync(string id);
    Task<List<Entities.Role>> DeleteByNamesAsync(IEnumerable<string> names);
}