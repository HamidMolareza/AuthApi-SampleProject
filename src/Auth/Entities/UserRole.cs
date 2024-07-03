using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class UserRole : IdentityUserRole<string> {
    public Role Role { get; set; } = default!;
    public User User { get; set; } = default!;
}