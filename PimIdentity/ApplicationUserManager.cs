using System;
using System.Diagnostics.Contracts;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using PimIdentity.Models;

namespace PimIdentity;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> options, IPasswordHasher<ApplicationUser> passwordHasher
        IEnumerable<IUserValidator<ApplicationUser>> userValidators)
        : base(store, options, passwordHasher, )
    {
    }

    /// <param name="options">
    ///     Optional
    /// </param>
    /// <param name="context">
    ///     Optional
    /// </param>
    /// <returns></returns>
    public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
    {
        Contract.Requires(options != null);
        Contract.Requires(context != null);

        var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<IdentityDatabaseContext>().Users));
        // Configure validation logic for usernames
        manager.UserValidator = new UserValidator<ApplicationUser>(manager)
        {
            AllowOnlyAlphanumericUserNames = false,
            RequireUniqueEmail = true
        };

        // Configure validation logic for passwords
        manager.PasswordValidator = new PasswordValidator
        {
            RequiredLength = 5,
            RequireNonLetterOrDigit = false,
            RequireDigit = false,
            RequireLowercase = false,
            RequireUppercase = false,
        };

        // Configure user lockout defaults
        manager.UserLockoutEnabledByDefault = true;
        manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
        manager.MaxFailedAccessAttemptsBeforeLockout = 5;

        // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
        // You can write your own provider and plug it in here.
        manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
        {
            MessageFormat = "Your security code is {0}"
        });
        manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
        {
            Subject = "Security Code",
            BodyFormat = "Your security code is {0}"
        });
        manager.EmailService = new EmailService();
        manager.SmsService = new SmsService();
        var dataProtectionProvider = options?.DataProtectionProvider;

        if (dataProtectionProvider != null)
        {
            manager.UserTokenProvider = 
                new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
        }
        return manager;
    }

    public static ApplicationUserManager CreateOutOfContext(IdentityDatabaseContextFactory databaseContextFactory)
    {
        return new ApplicationUserManager(databaseContextFactory.UserStore);
    }
}