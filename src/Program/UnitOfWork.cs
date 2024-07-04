using AuthApi.Auth.Entities;
using AuthApi.Auth.Services;
using AuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthApi.Program;

public class UnitOfWork(
    UserManager userManager,
    RoleManager<Role> roleManager,
    ITokenManager tokenManager,
    ISessionManager sessionManager,
    AppDbContext db
) : IUnitOfWork {
    public UserManager UserManager { get; } = userManager;
    public RoleManager<Role> RoleManager { get; } = roleManager;
    public ITokenManager TokenManager { get; } = tokenManager;
    public ISessionManager SessionManager { get; } = sessionManager;

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
}