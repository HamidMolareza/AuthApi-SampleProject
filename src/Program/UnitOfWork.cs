using AuthApi.Auth.Entities;
using AuthApi.Auth.Services.Role;
using AuthApi.Auth.Services.UserServices;
using AuthApi.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UserStore = AuthApi.Auth.Services.UserServices.UserStore;

namespace AuthApi.Program;

public class UnitOfWork : IUnitOfWork {
    public IRoleStore RoleStore { get; }
    public IUserStore UserStore { get; }

    private IDbContextTransaction? _transaction;
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db) {
        _db = db;

        var aspRoleStore = new RoleStore<Role>(db);
        RoleStore = new RoleStore(aspRoleStore, db);

        var aspUserStore = new UserStore<User>(db);
        UserStore = new UserStore(db, aspUserStore);
    }

    public async Task BeginTransactionAsync() {
        if (_transaction is not null) {
            throw new Exception(
                "Can not create new transaction while the previous transaction is not used and is not disposed.");
        }

        _transaction = await _db.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync() {
        if (_transaction is null) {
            throw new Exception(
                $"The transaction is not started and can not commit. Please use {nameof(BeginTransactionAsync)} first.");
        }

        try {
            await SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch {
            await RollbackTransactionAsync();
            throw;
        }
        finally {
            _transaction.Dispose();
        }
    }

    public async Task RollbackTransactionAsync() {
        if (_transaction is null) {
            throw new Exception(
                $"The transaction is not started and can not commit. Please use {nameof(BeginTransactionAsync)} first.");
        }

        await _transaction.RollbackAsync();
        _transaction.Dispose();
    }

    public Task<int> SaveChangesAsync() {
        return _db.SaveChangesAsync();
    }

    public void Dispose() {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}