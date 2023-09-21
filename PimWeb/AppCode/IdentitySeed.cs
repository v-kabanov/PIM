using System.Reflection;
using log4net;
using Microsoft.AspNetCore.Identity;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using IdentityRole = Raven.Identity.IdentityRole;
using IdentityUser = Raven.Identity.IdentityUser;

namespace PimWeb.AppCode;

public static class IdentitySeed
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
    public static async Task Seed(this IServiceProvider serviceProvider, SeedUsers users)
    {
        // Create the database if it doesn't exist.
        // Also, create our roles if they don't exist. Needed because we're doing some role-based auth in this demo.
        var docStore = serviceProvider.GetRequiredService<IDocumentStore>();
        
        docStore.EnsureExists();
        //docStore.EnsureRolesExist(new List<string> { IdentityConstants.AdminRoleName, IdentityConstants.ReaderRoleName, IdentityConstants.WriterRoleName });

        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

        foreach (var roleName in IdentityConstants.AllRoleNames)
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var ir = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!ir.Succeeded)
                    Log.ErrorFormat("'{0}' role creation: {1}", roleName, ir);
            }

        var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
        
        foreach (var seedUser in users.Users)
        {
            var user = await userManager.EnsureUser(seedUser.Name, seedUser.Email, seedUser.Password);
            
            if (user != null)
                foreach (var roleName in seedUser.Roles)
                    await userManager.AddToRoleInternalAsync(user, roleName);
        }
    }
    
    private static async Task<IdentityResult> AddToRoleInternalAsync(this UserManager<IdentityUser> userManager, IdentityUser user, string roleName)
    {
        var ir = await userManager.AddToRoleAsync(user, roleName);
        if (!ir.Succeeded)
            Log.ErrorFormat("Adding role '{0}' to user '{1}': {2}", roleName, user.UserName, ir);
        return ir;
    }
    
    private static async Task<IdentityUser> EnsureUser(this UserManager<IdentityUser> userManager, string name, string email, string password)
    {
        var user = await userManager.FindByNameAsync(name);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = name,
                Email = email,
                EmailConfirmed = true
            };
            var ir = await userManager.CreateAsync(user, password);
            if (!ir.Succeeded)
                Log.ErrorFormat("Creation of user '{0}': {1}", name, ir);
        }

        return user;
    }
}