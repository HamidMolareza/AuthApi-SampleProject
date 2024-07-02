using AuthApi.Helpers;

namespace AuthApi.Auth.Options;

public class AppPasswordOptions : OptionModel {
    public override string SectionName => "Password";

    public required bool RequireDigit { get; set; }
    public required bool RequireLowercase { get; set; }
    public required bool RequireNonAlphanumeric { get; set; }
    public required bool RequireUppercase { get; set; }
    public required short RequiredLength { get; set; }
    public required short RequiredUniqueChars { get; set; }
    public required int DefaultLockoutMinutes { get; set; }
    public required int MaxFailedAccessAttempts { get; set; }
    public required bool AllowedForNewUsers { get; set; }
    public required bool RequireUniqueEmail { get; set; }
    public required string AllowedUserNameCharacters { get; set; }
}