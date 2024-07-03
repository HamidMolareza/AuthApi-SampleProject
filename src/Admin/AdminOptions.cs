using AuthApi.Helpers;

namespace AuthApi.Admin;

public class AdminOptions : OptionModel {
    public override string SectionName => "Admin";
    public required string Email { get; set; }
    public required string Password { get; set; }
}