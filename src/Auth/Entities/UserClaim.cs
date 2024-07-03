using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class UserClaim : IdentityUserClaim<string> {
    public User User { get; set; } = default!;
}