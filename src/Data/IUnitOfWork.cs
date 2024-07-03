using System.Security.Claims;
using AuthApi.Auth.Entities;
using AuthApi.Auth.Options;
using AuthApi.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthApi.Data;

public interface IUnitOfWork {
    public UserManager UserManager { get; }
    public RoleManager<Role> RoleManager { get; }
    public JwtOptions JwtOptions { get; }
    public string CreateJwtToken(IEnumerable<Claim> claims);
    public Task<string> CreateJwtTokenAsync(User user, bool addRoleClaims = true);
    public Task<IDbContextTransaction> BeginTransaction();
    public Task CommitTransactionAsync();
    public Task RollbackTransactionAsync();
    public Task<int> SaveChangesAsync();
}