using System.Security.Claims;
using System.Text;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Options;
using AuthApi.Data;
using AuthApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Auth.Services;

public static class AuthServiceConfigurations {
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
        List<OptionModel> optionModels) {
        var jwtOptions = optionModels.GetOption<JwtOptions>();
        var passwordOptions = optionModels.GetOption<AppPasswordOptions>();

        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<UserManager>()
            .AddScoped<ITokenManager, TokenManager>()
            .AddScoped<ISessionManager, SessionManager>();

        services.AddAuthentication();

        services.Configure(IdentityConfigureOptions(passwordOptions));

        services.ConfigureJwt(jwtOptions);

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

            options.SignIn.RequireConfirmedAccount = appPasswordOptions.RequireConfirmedAccount;
            options.SignIn.RequireConfirmedEmail = appPasswordOptions.RequireConfirmedEmail;
            options.SignIn.RequireConfirmedPhoneNumber = appPasswordOptions.RequireConfirmedPhoneNumber;

            options.Stores.ProtectPersonalData = appPasswordOptions.ProtectPersonalData;
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

            // Hook into the events to add custom validation logic
            options.Events = new JwtBearerEvents {
                OnTokenValidated = async context => {
                    // Add custom logic here
                    // e.g., Check user roles, claims, or any other custom validation

                    var userPrincipal = context.Principal;
                    if (userPrincipal is null) {
                        context.Fail("Unauthorized"); // Mark the request as failed
                        return;
                    }

                    var sessionId = userPrincipal.FindFirstValue(Claims.SessionId);
                    if (sessionId is null) {
                        context.Fail("Unauthorized");
                        return;
                    }

                    var sessionManager = context.HttpContext.RequestServices.GetRequiredService<ISessionManager>();
                    var session = await sessionManager.GetByIdAsync(new Guid(sessionId));
                    if (session is null || !session.Active) {
                        context.Fail("Unauthorized");
                        return;
                    }
                },
                OnAuthenticationFailed = context => Task.CompletedTask,
                OnMessageReceived = context => Task.CompletedTask
            };
        });
    }
}