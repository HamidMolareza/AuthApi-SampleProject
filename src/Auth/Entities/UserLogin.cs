using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class UserLogin : IdentityUserLogin<string> {
    public User User { get; set; } = default!;
}