// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-18
// Comment  
// **********************************************************************************************/

using System;
using System.Diagnostics.Contracts;
using LiteDB;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;

namespace PimIdentity;

public class IdentityDatabaseContextFactory : IDisposable
{
    public LiteDbContext DbContext { get; }
    
    public IdentityDatabaseContextFactory(LiteDatabase database)
    {
        Contract.Requires(database != null);
        DbContext = new LiteDbContext(database);

        Roles = database.GetCollection<IdentityRole>("roles");
        Users = database.GetCollection<ApplicationUser>("users");

        UserStore = new LiteDbUserStore<ApplicationUser>(DbContext);
        RoleStore = new LiteDbRoleStore<IdentityRole>(DbContext);
    }

    public ILiteCollection<ApplicationUser> Users { get; }

    public ILiteCollection<IdentityRole> Roles { get; }

    public LiteDbUserStore<ApplicationUser> UserStore { get; }

    public LiteDbRoleStore<IdentityRole> RoleStore { get; }

    public IdentityDatabaseContext Create()
    {
        return new IdentityDatabaseContext(this);
    }

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
                DbContext?.LiteDatabase?.Dispose();
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