using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Identity;

public class AppUser : IdentityUser<int>
{
    /// <inheritdoc />
    public AppUser()
    {
    }

    /// <inheritdoc />
    public AppUser(string userName) : base(userName)
    {
    }
  
    public virtual ISet<AppRole> Roles { get; set; } = new HashSet<AppRole>();
  
    public virtual ISet<IdentityUserClaim<int>> Claims { get; set; } = new HashSet<IdentityUserClaim<int>>();
  
    public virtual ISet<AppUserLogin> Logins { get; set; } = new HashSet<AppUserLogin>();
  
    public virtual ISet<AppUserToken> Tokens { get; set; } = new HashSet<AppUserToken>();
}