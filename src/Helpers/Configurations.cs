using System.Reflection;

namespace AuthApi.Helpers;

public abstract class OptionModel {
    public abstract string SectionName { get; }
}

public static class Configurations {
    public static List<OptionModel> AddOptionModels(this IServiceCollection services, Assembly executingAssembly,
        ConfigurationManager configuration) {
        var optionModels = new List<OptionModel>();

        var optionModelType = typeof(OptionModel);
        var sectionNameProp = optionModelType.GetProperty(nameof(OptionModel.SectionName))!;

        // Get all types that implement IOptionModel interface
        var optionTypes = executingAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && optionModelType.IsAssignableFrom(type));

        foreach (var optionType in optionTypes) {
            var optionModelInstance = Activator.CreateInstance(optionType);

            var sectionName = sectionNameProp.GetValue(optionModelInstance) as string;
            if (string.IsNullOrEmpty(sectionName)) {
                throw new Exception(
                    $"Value of {nameof(OptionModel.SectionName)} in {optionType.Name} is null or empty.");
            }

            // Create the generic method for Configure<TOptions>
            var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "Configure" && m.GetParameters().Length == 2)?
                .MakeGenericMethod(optionType);

            configuration.GetSection(sectionName).Bind(optionModelInstance);
            optionModels.Add((OptionModel)optionModelInstance!);

            // Invoke the Configure<TOptions> method
            configureMethod!.Invoke(null, [services, configuration.GetSection(sectionName)]);
        }

        return optionModels;
    }
}