// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-18
// Comment  
// **********************************************************************************************/

using System;
using System.Diagnostics.Contracts;
using AspNet.Identity.LiteDB;
using LiteDB;
using Microsoft.AspNet.Identity;
using PimIdentity.Models;

namespace PimIdentity;

public class IdentityDatabaseContextFactory : IDisposable
{
    public IdentityDatabaseContextFactory(LiteDatabase database)
    {
        Contract.Requires(database != null);

        Roles = database.GetCollection<IdentityRole>("roles");
        Users = database.GetCollection<ApplicationUser>("users");

        UserStore = new UserStore<ApplicationUser>(Users);
        RoleStore = new RoleStore<IdentityRole>(Roles);
    }

    public LiteCollection<ApplicationUser> Users { get; }

    public LiteCollection<IdentityRole> Roles { get; }

    public IUserStore<ApplicationUser> UserStore { get; }

    public IRoleStore<IdentityRole> RoleStore { get; }

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