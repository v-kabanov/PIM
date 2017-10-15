using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.AspNet.Identity;
using AspNet.Identity.LiteDB;
using LiteDB;

namespace AspNetPim.Models
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

    public class DbContext : IDisposable
    {
        private readonly LiteDatabase _database;

        public DbContext()
        {
            _database = MvcApplication.AuthDatabase;
        }

        public static DbContext Create()
        {
            return new DbContext();
        }

        public LiteCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("users");

        public LiteCollection<IdentityRole> Roles => _database.GetCollection<IdentityRole>("roles");

        public void Dispose() { }
    }
}