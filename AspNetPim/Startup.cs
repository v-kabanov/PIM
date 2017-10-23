using System.Web.Mvc;
using Microsoft.Owin;
using Owin;
using PimIdentity;

[assembly: OwinStartup(typeof(AspNetPim.Startup))]
namespace AspNetPim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var databaseContextFactory = DependencyResolver.Current.GetService<IdentityDatabaseContextFactory>();

            var identityConfiguration = new IdentityConfiguration(databaseContextFactory);
            var task = identityConfiguration.EnsureDefaultUsersAndRolesAsync();

            ConfigureAuth(app);

            task.GetAwaiter().GetResult();
        }
    }
}
