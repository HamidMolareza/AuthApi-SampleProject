using AuthApi.Admin.Options;
using AuthApi.Auth;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Services.Role;
using AuthApi.Auth.Services.UserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OnRails.Extensions.OnFail;

namespace AuthApi.Data;

public static class DatabaseInitializer {
    public static async Task SeedDatabaseAsync(this WebApplication app) {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var roleManager = scope.ServiceProvider.GetRequiredService<IRoleManager>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminOptions>>();


        await CreateRolesAsync(roleManager);
        await CreateUsersAsync(userManager, adminOptions.Value);
    }

    private static async Task CreateUsersAsync(IUserManager userManager, AdminOptions options) {
        if (await userManager.AnyAsync()) return;

        var user = new User {
            Email = options.Email,
            EmailConfirmed = true,
            UserName = options.Email
        };
        var result = await userManager.CreateAsync(user, options.Password);
        if (!result.Succeeded) throw new Exception(GetErrorMessage(result.Errors));
        await userManager.AddRolesAsync(user.Id, [Roles.Administrator])
            .OnFailThrowException();
    }

    private static string GetErrorMessage(this IEnumerable<IdentityError> errors) =>
        string.Join('\n', errors.Select(e => e.Description));

    private static async Task CreateRolesAsync(IRoleManager roleManager) {
        if (await roleManager.AnyAsync()) return;

        foreach (var roleName in Roles.GetRoleNames()) {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateRolesAsync([roleName]);
        }
    }
}