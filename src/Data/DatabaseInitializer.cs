using AuthApi.Admin;
using AuthApi.Auth;
using AuthApi.Auth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AuthApi.Data;

public static class DatabaseInitializer {
    public static async Task SeedDatabaseAsync(this WebApplication app) {
        using var scope = app.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminOptions>>();


        await CreateRolesAsync(unitOfWork);
        await CreateUsersAsync(unitOfWork, adminOptions.Value);
    }

    private static async Task CreateUsersAsync(IUnitOfWork unitOfWork, AdminOptions options) {
        if (unitOfWork.UserManager.Users.Any()) return;

        var user = new User {
            Email = options.Email,
            EmailConfirmed = true,
            UserName = options.Email
        };
        var result = await unitOfWork.UserManager.CreateAsync(user, options.Password);
        if (!result.Succeeded) throw new Exception(GetErrorMessage(result.Errors));
        result = await unitOfWork.UserManager.AddToRoleAsync(user, Roles.Administrator);
        if (!result.Succeeded) throw new Exception(GetErrorMessage(result.Errors));
    }

    private static string GetErrorMessage(this IEnumerable<IdentityError> errors) =>
        string.Join('\n', errors.Select(e => e.Description));

    private static async Task CreateRolesAsync(IUnitOfWork unitOfWork) {
        if (unitOfWork.RoleManager.Roles.Any()) return;

        foreach (var roleName in Roles.GetRoleNames()) {
            if (!await unitOfWork.RoleManager.RoleExistsAsync(roleName))
                await unitOfWork.RoleManager.CreateAsync(new Role(roleName));
        }
    }
}