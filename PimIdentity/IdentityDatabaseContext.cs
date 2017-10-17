using System;
using System.Diagnostics.Contracts;
using AspNet.Identity.LiteDB;
using LiteDB;
using Microsoft.AspNet.Identity;
using PimIdentity.Models;

namespace PimIdentity
{
    public class IdentityDatabaseContext : IDisposable
    {

        public IdentityDatabaseContextFactory ContextFactory { get; }

        public IdentityDatabaseContext(IdentityDatabaseContextFactory contextFactory)
        {
            Contract.Requires(contextFactory != null);

            ContextFactory = contextFactory;
        }

        public LiteCollection<ApplicationUser> Users => ContextFactory.Users;

        public LiteCollection<IdentityRole> Roles => ContextFactory.Roles;

        public IUserStore<ApplicationUser> UserStore => ContextFactory.UserStore;

        public IRoleStore<IdentityRole> RoleStore => ContextFactory.RoleStore;

        public void Dispose() { }
    }
}