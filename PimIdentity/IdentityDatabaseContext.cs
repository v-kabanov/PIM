using System;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace PimIdentity;

public class IdentityDatabaseContext : IDisposable
{
    public LiteDbContext DbContext { get; }

    public IdentityDatabaseContext([NotNull] LiteDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        
        //Roles = database.GetCollection<IdentityRole>("roles");
        //Users = database.GetCollection<ApplicationUser>("users");

        RoleStore = new LiteDbRoleStore<IdentityRole>(DbContext);
        UserStore = new PimUserStore<ApplicationUser>(DbContext, RoleStore);
    }

    //public ILiteCollection<ApplicationUser> Users { get; }
    //public ILiteCollection<IdentityRole> Roles { get; }

    public IUserStore<ApplicationUser> UserStore { get; }

    public IRoleStore<IdentityRole> RoleStore { get; }


    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects).
                RoleStore?.Dispose();
                UserStore?.Dispose();
            }
            _disposedValue = true;
        }
    }

    // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~IdentityDatabaseContextFactory() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    public void Dispose()
    {
        Dispose(true);
        // uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
}