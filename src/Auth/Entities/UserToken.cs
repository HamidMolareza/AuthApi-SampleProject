using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class UserToken : IdentityUserToken<string> {
    public User User { get; set; } = default!;
}