using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Identity;

public class AppRole : IdentityRole<int>
{
    /// <inheritdoc />
    public AppRole()
    {
    }

    /// <inheritdoc />
    public AppRole(string roleName) : base(roleName)
    {
    }

    public virtual ISet<IdentityRoleClaim<int>> Claims { get; set; } = new HashSet<IdentityRoleClaim<int>>();
}