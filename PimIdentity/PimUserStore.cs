using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace PimIdentity;

public class PimUserStore<TUser, TRole>
    : LiteDbUserStore<TUser>
    , IUserRoleStore<TUser>
    where TUser : ApplicationUser, new()
    where TRole: IdentityRole
{
    private readonly IRoleStore<TRole> _roleStore;
    
    /// <inheritdoc />
    public PimUserStore(ILiteDbContext dbContext, [NotNull] IRoleStore<TRole> roleStore) : base(dbContext)
    {
        _roleStore = roleStore ?? throw new ArgumentNullException(nameof(roleStore));
    }

    async Task<bool> IUserRoleStore<TUser>.IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null) throw new ArgumentNullException(nameof (user));
        if (roleName == null) throw new ArgumentNullException(nameof (roleName));
        
        var role = await GetRequiredRoleAsync(roleName, cancellationToken).ConfigureAwait(false);
        
        return user.Roles.Contains(role.Name);
    }
    
    /// <summary>
    ///     Override to work around LiteDB store implementations substituting roles with their normalized keys which forces to use keys (e.g. uppercase)
    ///     rather than names in <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute"/>.
    /// </summary>
    /// <param name="user">
    ///     Mandatory
    /// </param>
    /// <param name="roleName">
    ///     <see cref="UserManager{TUser}"/> passes normalized name.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    async Task IUserRoleStore<TUser>.AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (user == null) throw new ArgumentNullException(nameof (user));
        if (roleName == null) throw new ArgumentNullException(nameof (roleName));
        
        var role = await GetRequiredRoleAsync(roleName, cancellationToken).ConfigureAwait(false);
        
        user.Roles.Add(role.Name);
    }

    /// <param name="roleName">
    ///     <see cref="UserManager{TUser}"/> passes normalized name here.
    /// </param>
    async Task IUserRoleStore<TUser>.RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (user == null) throw new ArgumentNullException(nameof (user));
        if (roleName == null) throw new ArgumentNullException(nameof (roleName));
        
        var role = await GetRequiredRoleAsync(roleName, cancellationToken).ConfigureAwait(false);
        
        user.Roles.Remove(role.Name);
    }
    
    private async Task<TRole> GetRequiredRoleAsync(string normalizedName, CancellationToken cancellationToken)
    {
        var role = await _roleStore.FindByNameAsync(normalizedName, cancellationToken).ConfigureAwait(false);
        if (role == null)
            throw new ArgumentException($"Role '{normalizedName}' does not exist.");
        return role;
    }
}