using AuthApi.Auth.Entities;
using AuthApi.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthApi.Program;

public interface IUnitOfWork {
    public UserManager UserManager { get; }
    public RoleManager<Role> RoleManager { get; }
    public ITokenManager TokenManager { get; }
    public Task<IDbContextTransaction> BeginTransaction();
    public Task CommitTransactionAsync();
    public Task RollbackTransactionAsync();
    public Task<int> SaveChangesAsync();
}