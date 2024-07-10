using AuthApi.Helpers;
using AuthApi.Helpers.Option;

namespace AuthApi.Admin.Options;

public class AdminOptions : OptionModel {
    public override string SectionName => "Admin";
    public required string Email { get; set; }
    public required string Password { get; set; }
}