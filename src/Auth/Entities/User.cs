using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class User : IdentityUser {
    public List<UserRole> UserRoles { get; set; } = default!;
    public List<UserClaim> UserClaims { get; set; } = default!;
    public List<UserLogin> Logins { get; set; } = default!;
    public List<UserToken> Tokens { get; set; } = default!;
    public List<Session> Sessions { get; set; } = default!;
}