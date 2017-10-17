using System;
using System.Diagnostics.Contracts;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using AspNet.Identity.LiteDB;
using CommonServiceLocator;
using LiteDB;

namespace PimIdentity.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class IdentityDatabaseContext : IDisposable
    {
        private static readonly Lazy<IdentityDatabaseContext> SingleInstance = new Lazy<IdentityDatabaseContext>(
            () => ServiceLocator.Current.GetInstance<IdentityDatabaseContext>());

        public IdentityDatabaseContext(LiteDatabase database)
        {
            Contract.Requires(database != null);
            
            Roles = database.GetCollection<IdentityRole>("roles");
            Users = database.GetCollection<ApplicationUser>("users");
        }

        public static IdentityDatabaseContext Create()
        {
            return SingleInstance.Value;
        }

        public LiteCollection<ApplicationUser> Users { get; }

        public LiteCollection<IdentityRole> Roles { get; }

        public void Dispose() { }
    }
}