using AspNet.Identity.LiteDB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using PimIdentity.Models;

namespace PimIdentity
{
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> store) : base(store)
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
            var dbContext = context?.Get<IdentityDatabaseContext>() ?? IdentityDatabaseContext.Create();
            var roleStore = new RoleStore<IdentityRole>(dbContext.Roles);
            return new ApplicationRoleManager(roleStore);
        }

        public static ApplicationRoleManager CreateOutOfContext() => new ApplicationRoleManager(
            new RoleStore<IdentityRole>(IdentityDatabaseContext.Create().Roles));
    }
}