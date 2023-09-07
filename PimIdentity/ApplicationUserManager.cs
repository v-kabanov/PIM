using System;
using System.Diagnostics.Contracts;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pim.CommonLib;

namespace PimIdentity;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> options = null, IServiceProvider serviceProvider = null)
        : base(store, options, new PasswordHasher<ApplicationUser>()
            , new UserValidator<ApplicationUser>().WrapInList()
            , new PasswordValidator<ApplicationUser>().WrapInList()
            , new UpperInvariantLookupNormalizer()
            , new IdentityErrorDescriber()
            , serviceProvider
            , null)
    {
    }

    public static IUserStore<ApplicationUser> CreateUserStore(ILiteDbContext databaseContext) => new LiteDbUserStore<ApplicationUser>(databaseContext);

    public static ApplicationUserManager Create(IServiceProvider serviceProvider, bool setOptions = true) 
    {
        Contract.Requires(serviceProvider != null);

        var identityDatabaseContext = serviceProvider.GetRequiredService<IdentityDatabaseContext>();
        
        return CreateInternal(identityDatabaseContext.UserStore, setOptions);
    }

    public static ApplicationUserManager CreateOutOfContext(IdentityDatabaseContextFactory databaseContextFactory, bool setOptions = true)
        => CreateInternal(databaseContextFactory.UserStore, setOptions);
    
    private static ApplicationUserManager CreateInternal([NotNull] IUserStore<ApplicationUser> store, bool setOptions)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));
        
        var options = setOptions
            ? GetDefaultOptions()
            : null;

        return new ApplicationUserManager(store, options);
    }
    
    public static IOptions<IdentityOptions> GetDefaultOptions()
    {
        var result = new IdentityOptions();
        SetOptions(result);
        return new OptionsWrapper<IdentityOptions>(result);
    }
    
    public static void SetOptions(IdentityOptions options)
    {
        options.Password = new PasswordOptions
        {
            RequireDigit = false
            , RequireLowercase = false
            , RequireUppercase = false
            , RequiredUniqueChars = 4
            , RequiredLength = 6
            , RequireNonAlphanumeric = false
        };
        options.Stores = new StoreOptions { MaxLengthForKeys = 5 };
        //options.User = new UserOptions { RequireUniqueEmail = true };
    }
}