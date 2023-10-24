using System.Reflection;
using log4net;
using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode;

public static class IdentitySeed
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
    public static async Task Seed(this IServiceProvider serviceProvider, SeedUsers users)
    {
        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole<int>>>();

        foreach (var roleName in PimIdentityConstants.AllRoleNames)
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var ir = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!ir.Succeeded)
                    Log.ErrorFormat("'{0}' role creation: {1}", roleName, ir);
            }

        var userManager = serviceProvider.GetService<UserManager<IdentityUser<int>>>();
        
        foreach (var seedUser in users.Users)
        {
            var user = await userManager.EnsureUser(seedUser.Name, seedUser.Email, seedUser.Password);
            
            if (user != null)
                foreach (var roleName in seedUser.Roles)
                    await userManager.AddToRoleInternalAsync(user, roleName);
        }
    }
    
    private static async Task<IdentityResult> AddToRoleInternalAsync(this UserManager<IdentityUser<int>> userManager, IdentityUser<int> user, string roleName)
    {
        var ir = await userManager.AddToRoleAsync(user, roleName);
        if (!ir.Succeeded)
            Log.ErrorFormat("Adding role '{0}' to user '{1}': {2}", roleName, user.UserName, ir);
        return ir;
    }
    
    private static async Task<IdentityUser<int>> EnsureUser(this UserManager<IdentityUser<int>> userManager, string name, string email, string password)
    {
        var user = await userManager.FindByNameAsync(name);
        if (user == null)
        {
            user = new IdentityUser<int>
            {
                UserName = name,
                Email = email,
                EmailConfirmed = true
            };
            var ir = await userManager.CreateAsync(user, password);
            if (!ir.Succeeded)
            {
                Log.ErrorFormat("Creation of user '{0}': {1}", name, ir);
                return null;
            }
        }

        return user;
    }
}