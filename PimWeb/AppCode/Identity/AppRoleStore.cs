using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NHibernate;
using NHibernate.Linq;

namespace PimWeb.AppCode.Identity;

public class AppRoleStore
    : IQueryableRoleStore<AppRole>
    , IRoleClaimStore<AppRole>
    , IRoleStore<AppRole>
{
    private bool _disposed;

    private readonly NHibernate.ISession _session;
    
    private readonly IdentityErrorDescriber _errorDescriber;

    public AppRoleStore(NHibernate.ISession session, IdentityErrorDescriber errorDescriber)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _errorDescriber = errorDescriber ?? throw new ArgumentNullException(nameof(errorDescriber));
    }

    /// <inheritdoc />
    public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        await _session.SaveAsync(role, cancellationToken).ConfigureAwait(false);
        await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
        
        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
    {
        role.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        await _session.MergeAsync(role, cancellationToken).ConfigureAwait(false);
        await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        
        await _session.DeleteAsync(role, cancellationToken).ConfigureAwait(false);
        await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
        return IdentityResult.Success;
    }

    /// <inheritdoc />
    public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Id.ToString());

    /// <inheritdoc />
    public Task<string> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Name);

    /// <inheritdoc />
    public Task SetRoleNameAsync(AppRole role, string roleName, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(roleName));
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        role.Name = roleName;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.NormalizedName);

    /// <inheritdoc />
    public Task SetNormalizedRoleNameAsync(AppRole role, string normalizedName, CancellationToken cancellationToken)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<AppRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var id = int.Parse(roleId);
      
        return _session.GetAsync<AppRole>(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AppRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken)
        => Roles.FirstOrDefaultAsync(u => u.NormalizedName == normalizedName, cancellationToken);

    /// <inheritdoc />
    public IQueryable<AppRole> Roles => _session.Query<AppRole>();

    /// <inheritdoc />
    public Task<IList<Claim>> GetClaimsAsync(AppRole role, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (role == null)
            throw new ArgumentNullException(nameof (role));
      
        return Task.FromResult((IList<Claim>)role.Claims.Select(x => x.ToClaim()).ToList());
    }

    /// <inheritdoc />
    public Task AddClaimAsync(AppRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (role == null)
            throw new ArgumentNullException(nameof (role));
        if (claim == null)
            throw new ArgumentNullException(nameof (claim));

        var roleClaim = new IdentityRoleClaim<int>
        {
            RoleId = role.Id
            , ClaimType = claim.Type
            , ClaimValue = claim.Value
        };

        role.Claims.Add(roleClaim);
        
        return FlushChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveClaimAsync(AppRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        var existingClaim = role.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
        if (null != existingClaim)
        {
            role.Claims.Remove(existingClaim);
            await _session.DeleteAsync(existingClaim, cancellationToken).ConfigureAwait(false);
        }
        
        await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task FlushChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTransaction = _session.GetCurrentTransaction();
        if (currentTransaction != null)
            await currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        else
            await _session.FlushAsync(cancellationToken).ConfigureAwait(false);
        _session.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }
    
    public void Dispose()
    {
        _session?.Dispose();
        _disposed = true;
    }
}