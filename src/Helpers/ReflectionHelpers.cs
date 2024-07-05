using System.Reflection;

namespace AuthApi.Helpers;

public static class ReflectionHelpers {
    public static List<T> GetProperties<T>(Type rolesType) {
        return rolesType
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(T))
            .Select(prop => (T)prop.GetValue(null)!)
            .ToList();
    }

    public static List<T> GetFields<T>(Type rolesType) {
        return rolesType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(T))
            .Select(field => (T)field.GetValue(null)!)
            .ToList();
    }
}