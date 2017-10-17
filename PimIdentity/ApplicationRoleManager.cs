using System.Diagnostics.Contracts;
using AspNet.Identity.LiteDB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

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
}