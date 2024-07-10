using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class Role : IdentityRole {
    public Role() { }

    public Role(string roleName) : base(roleName) {
        NormalizedName = roleName.ToUpper();
    }

    public List<RoleClaim> RoleClaims { get; set; } = default!;
    public List<UserRole> UserRoles { get; set; } = default!;

    public override string NormalizedName { get; set; }
}