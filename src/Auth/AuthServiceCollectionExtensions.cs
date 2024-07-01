using System.Text;
using AuthApi.Auth.Options;
using AuthApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Auth;

public static class AuthServiceConfigurations {
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, JwtOptions jwtOptions, AppPasswordOptions appPasswordOptions) {
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication();

        services.Configure(IdentityConfigureOptions(appPasswordOptions));

        services.ConfigureJwt(jwtOptions);

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    private static Action<IdentityOptions> IdentityConfigureOptions(AppPasswordOptions appPasswordOptions) {
        return options => {
            // Password settings.
            options.Password.RequireDigit = appPasswordOptions.RequireDigit;
            options.Password.RequireLowercase = appPasswordOptions.RequireLowercase;
            options.Password.RequireNonAlphanumeric = appPasswordOptions.RequireNonAlphanumeric;
            options.Password.RequireUppercase = appPasswordOptions.RequireUppercase;
            options.Password.RequiredLength = appPasswordOptions.RequiredLength;
            options.Password.RequiredUniqueChars = appPasswordOptions.RequiredUniqueChars;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(appPasswordOptions.DefaultLockoutMinutes);
            options.Lockout.MaxFailedAccessAttempts = appPasswordOptions.MaxFailedAccessAttempts;
            options.Lockout.AllowedForNewUsers = appPasswordOptions.AllowedForNewUsers;

            // User settings.
            options.User.AllowedUserNameCharacters = appPasswordOptions.AllowedUserNameCharacters;
            options.User.RequireUniqueEmail = appPasswordOptions.RequireUniqueEmail;
        };
    }

    private static void ConfigureJwt(this IServiceCollection services, JwtOptions jwtOptions) {
        services.AddAuthentication(opt => {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new
                    SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
            };
        });
    }
}