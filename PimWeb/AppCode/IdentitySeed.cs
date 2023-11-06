using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PimWeb.AppCode.Identity;

namespace PimWeb.AppCode;

public static class IdentitySeed
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
    public static async Task Seed(this IServiceProvider serviceProvider, SeedUsers users)
    {
        var roleManager = serviceProvider.GetService<RoleManager<AppRole>>();

        foreach (var roleName in PimIdentityConstants.AllRoleNames)
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var ir = await roleManager.CreateAsync(new AppRole(roleName));
                if (!ir.Succeeded)
                    Log.ErrorFormat("'{0}' role creation: {1}", roleName, ir);
            }

        var userManager = serviceProvider.GetService<UserManager<AppUser>>();
        
        foreach (var seedUser in users.Users)
        {
            var user = await userManager.EnsureUser(seedUser.Name, seedUser.Email, seedUser.Password);
            
            if (user != null)
                foreach (var roleName in seedUser.Roles)
                    await userManager.AddToRoleInternalAsync(user, roleName);
        }
    }
    
    private static async Task<IdentityResult> AddToRoleInternalAsync(this UserManager<AppUser> userManager, AppUser user, string roleName)
    {
        var ir = await userManager.AddToRoleAsync(user, roleName);
        if (!ir.Succeeded)
            Log.ErrorFormat("Adding role '{0}' to user '{1}': {2}", roleName, user.UserName, ir);
        return ir;
    }
    
    private static async Task<AppUser> EnsureUser(this UserManager<AppUser> userManager, string name, string email, string password)
    {
        var user = await userManager.FindByNameAsync(name);
        if (user == null)
        {
            user = new AppUser
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