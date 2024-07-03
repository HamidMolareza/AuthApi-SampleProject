using System.Reflection;

namespace AuthApi.Auth;

public static class Roles {
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";

    public static List<string> GetRoleNames() {
        var rolesType = typeof(Roles);

        // Get values of fields and props
        var roles = rolesType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(field => field.GetValue(null) as string)
            .ToList();
        roles.AddRange(rolesType
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(string))
            .Select(prop => prop.GetValue(null) as string)
        );

        return roles.Where(role => !string.IsNullOrEmpty(role)).ToList()!;
    }
}