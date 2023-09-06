using System.Diagnostics.Contracts;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Owin;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace PimIdentity;

public class ApplicationRoleManager : RoleManager<IdentityRole>
{
    public ApplicationRoleManager(IRoleStore<IdentityRole> store) : base(store)
    {
    }

    /// <param name="options">
    ///     Optional
    /// </param>
    /// <param name="context">
    ///     Optional
    /// </param>
    /// <returns></returns>
    public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
    {
        Contract.Requires(options != null);
        Contract.Requires(context != null);

        var dbContext = context.Get<IdentityDatabaseContext>();
        var roleStore = new RoleStore<IdentityRole>(dbContext.Roles);
        return new ApplicationRoleManager(roleStore);
    }

    public static ApplicationRoleManager CreateOutOfContext(IdentityDatabaseContextFactory databaseContextFactory)
    {
        Contract.Requires(databaseContextFactory != null);

        return new ApplicationRoleManager(databaseContextFactory.RoleStore);
    }
}