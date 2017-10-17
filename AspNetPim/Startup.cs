using System.Web.Mvc;
using AspNet.Identity.LiteDB;
using FulltextStorageLib.Util;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Owin;
using PimIdentity;
using PimIdentity.Models;

[assembly: OwinStartup(typeof(AspNetPim.Startup))]
namespace AspNetPim
{
    public partial class Startup
    {
        private const string AdminRoleName = "Admin";

        private ApplicationRoleManager _roleManager;
        private ApplicationUserManager _userManager;

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var databaseContextFactory = DependencyResolver.Current.GetService<IdentityDatabaseContextFactory>();

            _roleManager = ApplicationRoleManager.CreateOutOfContext(databaseContextFactory);
            _userManager = ApplicationUserManager.CreateOutOfContext(databaseContextFactory);

            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            var adminResult = EnsureRole(AdminRoleName);

            if (adminResult.created)
            {
                var defaultAdminUser = new ApplicationUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                };

                var userResult = _userManager.Create(defaultAdminUser, "password");

                if (userResult.Succeeded)
                    _userManager.AddToRole(defaultAdminUser.Id, AdminRoleName);
            }

            EnsureRole("Reader");
            EnsureRole("Writer");
        }

        private (bool ensured, bool created) EnsureRole(string name)
        {
            Check.DoRequireArgumentNotNull(name, nameof(name));

            if (!_roleManager.RoleExists(name))
            {
                return (ensured: _roleManager.Create(new IdentityRole(name)).Succeeded, created: true);
            }

            return (true, false);
        }
    }
}
