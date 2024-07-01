using AuthApi.Helpers;

namespace AuthApi.Auth.Options;

public class JwtOptions : OptionModel {
    public override string SectionName => "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SecretKey { get; init; }
    public required long ExpiresInSeconds { get; init; }
}