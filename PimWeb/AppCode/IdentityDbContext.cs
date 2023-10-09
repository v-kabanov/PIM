using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PimWeb.AppCode;

public class IdentityDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
{
    public IdentityDbContext()
    {
    }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }
}
