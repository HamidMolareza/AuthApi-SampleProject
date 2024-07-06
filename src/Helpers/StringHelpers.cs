namespace AuthApi.Helpers;

public static class StringHelpers {
    public static List<string> GetUniqueLowerCase(this IEnumerable<string> names) {
        return names.Distinct()
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name.ToLower())
            .ToList();
    }
}