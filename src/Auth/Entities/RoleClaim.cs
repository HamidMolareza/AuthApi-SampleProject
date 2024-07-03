using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class RoleClaim : IdentityRoleClaim<string> {
    public Role Role { get; set; } = default!;
}