using AuthApi.Helpers;

namespace AuthApi.Auth.Options;

public class RefreshTokenOptions : OptionModel {
    public override string SectionName => "RefreshToken";
    public int ExpiresInHours { get; set; }
    public int Length { get; set; }
}