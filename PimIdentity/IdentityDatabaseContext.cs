using System;
using System.Diagnostics.Contracts;
using AspNetCore.Identity.LiteDB.Models;
using LiteDB;
using Microsoft.AspNetCore.Identity;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace PimIdentity;

public class IdentityDatabaseContext : IDisposable
{
    public IdentityDatabaseContextFactory ContextFactory { get; }

    public IdentityDatabaseContext(IdentityDatabaseContextFactory contextFactory)
    {
        Contract.Requires(contextFactory != null);

        ContextFactory = contextFactory;
    }

    public ILiteCollection<ApplicationUser> Users => ContextFactory.Users;

    public ILiteCollection<IdentityRole> Roles => ContextFactory.Roles;

    public IUserStore<ApplicationUser> UserStore => ContextFactory.UserStore;

    public IRoleStore<IdentityRole> RoleStore => ContextFactory.RoleStore;

    public void Dispose() { }
}