using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuthApi.Auth.Entities;

public sealed class User : IdentityUser {
    [MaxLength(100)] public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpireTime { get; set; }

    public List<UserRole> UserRoles { get; set; } = default!;
    public List<UserClaim> UserClaims { get; set; } = default!;

    public List<UserLogin> Logins { get; set; } = default!;
    public List<UserToken> Tokens { get; set; } = default!;
}