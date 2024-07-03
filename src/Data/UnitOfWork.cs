using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Options;
using AuthApi.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Data;

public class UnitOfWork(
    UserManager userManager,
    RoleManager<Role> roleManager,
    IOptions<JwtOptions> jwtOptions,
    AppDbContext db
) : IUnitOfWork {
    public UserManager UserManager { get; } = userManager;
    public RoleManager<Role> RoleManager { get; } = roleManager;
    public JwtOptions JwtOptions { get; } = jwtOptions.Value;

    public Task<IDbContextTransaction> BeginTransaction() {
        return db.Database.BeginTransactionAsync();
    }

    public Task CommitTransactionAsync() {
        return db.Database.CommitTransactionAsync();
    }

    public Task RollbackTransactionAsync() {
        return db.Database.RollbackTransactionAsync();
    }

    public Task<int> SaveChangesAsync() {
        return db.SaveChangesAsync();
    }

    public string CreateJwtToken(IEnumerable<Claim> claims) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(JwtOptions.SecretKey);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(JwtOptions.ExpiresInSeconds),
            SigningCredentials = credentials,
            Issuer = JwtOptions.Issuer,
            Audience = JwtOptions.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<string> CreateJwtTokenAsync(User user, bool addRoleClaims = true) {
        var claims = await GetClaimsAsync(user, addRoleClaims);
        return CreateJwtToken(claims);
    }

    private async Task<List<Claim>> GetClaimsAsync(User user, bool addRoleClaims = true) {
        //Avoid sensitive information (e.g., passwords) and large or non-essential data to maintain security and efficiency.

        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.Id),
        };

        if (addRoleClaims) {
            var roles = await UserManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        return claims;
    }
}