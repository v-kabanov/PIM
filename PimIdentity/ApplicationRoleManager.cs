using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Pim.CommonLib;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace PimIdentity;

public class ApplicationRoleManager : RoleManager<IdentityRole>
{
    public ApplicationRoleManager(IRoleStore<IdentityRole> store)
        : base(
            store
            , new RoleValidator<IdentityRole>().WrapInList()
            , new UpperInvariantLookupNormalizer()
            , new IdentityErrorDescriber()
            , null)
    {
    }

    /// <param name="serviceProvider">
    ///     Mandatory
    /// </param>
    public static ApplicationRoleManager Create(IServiceProvider serviceProvider)
    {
        Contract.Requires(serviceProvider != null);

        var identityDatabaseContext = serviceProvider.GetRequiredService<IdentityDatabaseContext>();
        
        return new ApplicationRoleManager(identityDatabaseContext.RoleStore);
    }

    public static ApplicationRoleManager CreateOutOfContext(IdentityDatabaseContextFactory databaseContextFactory)
    {
        Contract.Requires(databaseContextFactory != null);

        return new ApplicationRoleManager(databaseContextFactory.RoleStore);
    }
}