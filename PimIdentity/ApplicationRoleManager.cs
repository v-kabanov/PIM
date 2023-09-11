using System;
using System.Diagnostics.Contracts;
using AspNetCore.Identity.LiteDB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pim.CommonLib;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace PimIdentity;

public class ApplicationRoleManager : RoleManager<IdentityRole>
{
    public ApplicationRoleManager(IRoleStore<IdentityRole> store, IServiceProvider serviceProvider = null)
        : base(
            store
            , new RoleValidator<IdentityRole>().WrapInList()
            , new UpperInvariantLookupNormalizer()
            , new IdentityErrorDescriber()
            , serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<RoleManager<IdentityRole>>()
              ?? new Logger<RoleManager<IdentityRole>>(new LoggerFactory()))
    {
    }

    /// <param name="serviceProvider">
    ///     Mandatory
    /// </param>
    public static ApplicationRoleManager Create(IServiceProvider serviceProvider)
    {
        Contract.Requires(serviceProvider != null);

        var identityDatabaseContext = serviceProvider.GetRequiredService<IdentityDatabaseContext>();
        
        return new ApplicationRoleManager(identityDatabaseContext.RoleStore, serviceProvider);
    }

    public static ApplicationRoleManager CreateOutOfContext(IdentityDatabaseContextFactory databaseContextFactory)
    {
        Contract.Requires(databaseContextFactory != null);

        return new ApplicationRoleManager(databaseContextFactory.RoleStore);
    }
}