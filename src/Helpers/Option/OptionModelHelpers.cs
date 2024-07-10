using System.Reflection;

namespace AuthApi.Helpers.Option;

public static class OptionModelHelpers {
    public static T GetOption<T>(this IEnumerable<OptionModel> optionModels) where T : class {
        var optionModel = optionModels.First(option => option.GetType() == typeof(T));
        return (optionModel as T)!;
    }

    public static List<OptionModel> AddOptionModels(this IServiceCollection services, Assembly executingAssembly,
        ConfigurationManager configuration) {
        var optionModelType = typeof(OptionModel);
        var sectionNameProp = optionModelType.GetProperty(nameof(OptionModel.SectionName))!;

        // Get all OptionModel types with given assembly
        var optionTypes = executingAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && optionModelType.IsAssignableFrom(type));

        // Add and get value of each OptionModel
        var optionModels = optionTypes
            .Select(optionType => services.AddOptionModel(configuration, optionType, sectionNameProp))
            .ToList();

        return optionModels;
    }

    private static OptionModel AddOptionModel(this IServiceCollection services,
        IConfiguration configuration, Type optionType, PropertyInfo sectionNameProp) {
        var optionModelInstance = Activator.CreateInstance(optionType);

        var sectionName = GetSectionName(optionType, sectionNameProp, optionModelInstance);

        // Create the generic method for Configure<TOptions>
        var configureMethod = GetConfigureMethod(optionType);

        // Invoke the Configure<TOptions> method
        configureMethod!.Invoke(null, [services, configuration.GetSection(sectionName)]);

        configuration.GetSection(sectionName).Bind(optionModelInstance);
        return (OptionModel)optionModelInstance!;
    }

    private static MethodInfo? GetConfigureMethod(Type optionType) {
        var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "Configure" && m.GetParameters().Length == 2)?
            .MakeGenericMethod(optionType);
        return configureMethod;
    }

    private static string GetSectionName(Type optionType, PropertyInfo sectionNameProp, object? optionModelInstance) {
        var sectionName = sectionNameProp.GetValue(optionModelInstance) as string;
        if (string.IsNullOrEmpty(sectionName)) {
            throw new Exception(
                $"Value of {nameof(OptionModel.SectionName)} in {optionType.Name} is null or empty.");
        }

        return sectionName;
    }
}