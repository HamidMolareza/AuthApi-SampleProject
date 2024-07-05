using AuthApi.Helpers;

namespace AuthApi.Auth;

public static class Roles {
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";

    public static List<string> GetRoleNames() {
        var rolesType = typeof(Roles);

        // Get values of fields and props
        var roles = ReflectionHelpers.GetFields<string>(rolesType);
        roles.AddRange(ReflectionHelpers.GetProperties<string>(rolesType));

        return roles.Where(role => !string.IsNullOrEmpty(role)).ToList()!;
    }
}