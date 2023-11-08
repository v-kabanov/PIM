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

public class AppUserStore : 
    IUserLoginStore<AppUser>,
    IUserClaimStore<AppUser>,
    IUserPasswordStore<AppUser>,
    IUserSecurityStampStore<AppUser>,
    IUserEmailStore<AppUser>,
    IUserLockoutStore<AppUser>,
    IUserPhoneNumberStore<AppUser>,
    IQueryableUserStore<AppUser>,
    IUserTwoFactorStore<AppUser>,
    IUserAuthenticationTokenStore<AppUser>,
    IUserAuthenticatorKeyStore<AppUser>,
    IUserTwoFactorRecoveryCodeStore<AppUser>,
    IProtectedUserStore<AppUser>,
    IUserRoleStore<AppUser>,
    IDisposable
  {
    private bool _disposed;

    private readonly NHibernate.ISession _session;
    
    private IdentityErrorDescriber _errorDescriber;

    public AppUserStore(NHibernate.ISession session, IdentityErrorDescriber errorDescriber)
    {
      _session = session ?? throw new ArgumentNullException(nameof (session));
      _errorDescriber = errorDescriber ?? throw new ArgumentNullException(nameof(errorDescriber));
    }

    public IQueryable<AppUser> Users => _session.Query<AppUser>();

    private IQueryable<AppRole> Roles => _session.Query<AppRole>();

    /// <summary>
    /// Throws if this class has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
      if (_disposed)
      {
        throw new ObjectDisposedException(GetType().Name);
      }
    }

    public void Dispose()
    {
      _session?.Dispose();
      _disposed = true;
    }
    
    protected virtual IdentityUserClaim<int> CreateUserClaim(AppUser user, Claim claim)
    {
        var userClaim = new IdentityUserClaim<int> { UserId = user.Id };
        userClaim.InitializeFromClaim(claim);
        user.Claims.Add(userClaim);
        
        return userClaim;
    }

    protected virtual AppUserLogin CreateUserLogin(AppUser user, UserLoginInfo login)
    {
        var result = new AppUserLogin
        {
            UserId = user.Id,
            ProviderKey = login.ProviderKey,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName
        };
        user.Logins.Add(result);
        return result;
    }

    protected virtual AppUserToken CreateUserToken(AppUser user, string loginProvider, string name, string? value)
    {
        var result = new AppUserToken
        {
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name,
            Value = value
        };
        user.Tokens.Add(result);
        
        return result;
    }

    /// <inheritdoc />
    public Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken)
    {
      user.PasswordHash = passwordHash;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.PasswordHash);

    /// <inheritdoc />
    public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.PasswordHash != null);

    /// <inheritdoc />
    public Task SetSecurityStampAsync(AppUser user, string stamp, CancellationToken cancellationToken)
    {
      user.SecurityStamp = stamp;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetSecurityStampAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.SecurityStamp);

    /// <inheritdoc />
    public Task SetEmailAsync(AppUser user, string email, CancellationToken cancellationToken)
    {
      user.Email = email;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetEmailAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.Email);

    /// <inheritdoc />
    public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.EmailConfirmed);

    /// <inheritdoc />
    public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
    {
      user.EmailConfirmed = confirmed;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.NormalizedEmail);

    /// <inheritdoc />
    public Task SetNormalizedEmailAsync(AppUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
      user.NormalizedEmail = normalizedEmail;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.LockoutEnd);

    /// <inheritdoc />
    public Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
      user.LockoutEnd = lockoutEnd;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> IncrementAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(++user.AccessFailedCount);

    /// <inheritdoc />
    public Task ResetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
    {
      user.AccessFailedCount = 0;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int> GetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.AccessFailedCount);

    /// <inheritdoc />
    public Task<bool> GetLockoutEnabledAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.LockoutEnabled);

    /// <inheritdoc />
    public Task SetLockoutEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
    {
      user.LockoutEnabled = enabled;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetPhoneNumberAsync(AppUser user, string phoneNumber, CancellationToken cancellationToken)
    {
      user.PhoneNumber = phoneNumber;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetPhoneNumberAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.PhoneNumber);

    /// <inheritdoc />
    public Task<bool> GetPhoneNumberConfirmedAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.PhoneNumberConfirmed);

    /// <inheritdoc />
    public Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
    {
      user.PhoneNumberConfirmed = confirmed;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled, CancellationToken cancellationToken)
    {
      user.TwoFactorEnabled = enabled;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> GetTwoFactorEnabledAsync(AppUser user, CancellationToken cancellationToken)
      => Task.FromResult(user.TwoFactorEnabled);

    /// <inheritdoc />
    public async Task RemoveTokenAsync(AppUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
      var token = user.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);
      if (token != null)
      {
        user.Tokens.Remove(token);
        await _session.DeleteAsync(token, cancellationToken).ConfigureAwait(false);
        await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
      }
    }

    /// <inheritdoc />
    public Task<string> GetTokenAsync(AppUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
      var token = user.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);
      return Task.FromResult(token?.Value);
    }

    private const string InternalLoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
    private const string RecoveryCodeTokenName = "RecoveryCodes";

    /// <inheritdoc />
    public  Task SetAuthenticatorKeyAsync(AppUser user, string key, CancellationToken cancellationToken)
      => SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);
      

    /// <inheritdoc />
    public Task<string> GetAuthenticatorKeyAsync(AppUser user, CancellationToken cancellationToken)
      => GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);

    /// <inheritdoc />
    public Task ReplaceCodesAsync(AppUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
      var mergedCodes = string.Join(";", recoveryCodes);
      return SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> RedeemCodeAsync(AppUser user, string code, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();

      if (user == null)
        throw new ArgumentNullException(nameof(user));

      if (code == null)
        throw new ArgumentNullException(nameof(code));

      var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken)
        .ConfigureAwait(false) ?? "";
      
      var splitCodes = mergedCodes.Split(';');
      if (splitCodes.Contains(code))
      {
        var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
        await ReplaceCodesAsync(user, updatedCodes, cancellationToken).ConfigureAwait(false);
        return true;
      }
      
      return false;
    }

    /// <inheritdoc />
    public async Task<int> CountCodesAsync(AppUser user, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();

      if (user == null)
        throw new ArgumentNullException(nameof(user));

      var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken)
        .ConfigureAwait(false) ?? "";
      
      if (mergedCodes.Length > 0)
        return mergedCodes.Split(';').Length;

      return 0;
    }

    public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.Id.ToString());
    }

    public virtual Task<string> GetUserNameAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        return Task.FromResult(user.UserName);
    }

    public virtual Task SetUserNameAsync(AppUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public virtual Task<string> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        return Task.FromResult(user.NormalizedUserName);
    }

    public virtual Task SetNormalizedUserNameAsync(AppUser user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      await _session.SaveAsync(user, cancellationToken).ConfigureAwait(false);
      await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
      
      return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      
      if (!await Users.AnyAsync(u => u.Id == user.Id, cancellationToken).ConfigureAwait(false))
        return IdentityResult.Failed(new IdentityError()
        {
          Code = "UserNotExist",
          Description = "User with id " + user.Id + " does not exists!"
        });
      
      user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
      
      await _session.MergeAsync(user, cancellationToken).ConfigureAwait(false);
      await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
      
      return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      
      await _session.DeleteAsync(user, cancellationToken).ConfigureAwait(false);
      await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
      
      return IdentityResult.Success;
    }

    public Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var id = int.Parse(userId);
      
      return _session.GetAsync<AppUser>(id, cancellationToken);
    }

    public Task<AppUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        => Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);

    private Task<AppRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
      => Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);

    public async Task AddToRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (string.IsNullOrWhiteSpace(normalizedRoleName))
        throw new ArgumentNullException(nameof (normalizedRoleName));
      
      var role = await FindRoleAsync(normalizedRoleName, cancellationToken).ConfigureAwait(false);
      if (role == null)
        throw new InvalidOperationException("Role " + normalizedRoleName + " not found!");

      user.Roles.Add(role);
      
      await FlushChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveFromRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (string.IsNullOrWhiteSpace(normalizedRoleName))
        throw new ArgumentNullException(nameof (normalizedRoleName));
      
      var role = await FindRoleAsync(normalizedRoleName, cancellationToken).ConfigureAwait(false);
      if (role == null)
        return;
      
      user.Roles.Remove(role);
      
      await FlushChangesAsync(cancellationToken);
    }

    public Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      
      return Task.FromResult<IList<string>>(user.Roles.Select(x => x.Name).ToList());
    }

    public Task<bool> IsInRoleAsync(AppUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (string.IsNullOrWhiteSpace(normalizedRoleName))
        throw new ArgumentNullException(nameof (normalizedRoleName));
      
      return Task.FromResult(user.Roles.Any(x => x.NormalizedName == normalizedRoleName));
    }

    public Task<IList<Claim>> GetClaimsAsync(AppUser user, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      
      return Task.FromResult((IList<Claim>)user.Claims.Select(x => x.ToClaim()).ToList());
    }

    public Task AddClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (claims == null)
        throw new ArgumentNullException(nameof (claims));
      
      foreach (var claim in claims)
        CreateUserClaim(user, claim);
      return FlushChangesAsync(cancellationToken);
    }

    public Task ReplaceClaimAsync(AppUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (claim == null)
        throw new ArgumentNullException(nameof (claim));
      if (newClaim == null)
        throw new ArgumentNullException(nameof (newClaim));
      
      foreach (var existingClaim in user.Claims.Where(x => x.ClaimType == claim.Type && x.ClaimValue == newClaim.Value))
      {
        existingClaim.ClaimType = newClaim.Type;
        existingClaim.ClaimValue = newClaim.Value;
      }
      
      return FlushChangesAsync(cancellationToken);
    }

    public async Task RemoveClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (claims == null)
        throw new ArgumentNullException(nameof (claims));
      
      foreach (var claim in claims)
      {
        var existingClaim = user.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
        if (null != existingClaim)
        {
          user.Claims.Remove(existingClaim);
          await _session.DeleteAsync(existingClaim, cancellationToken).ConfigureAwait(false);
        }
      }
      
      await FlushChangesAsync(cancellationToken);
    }

    public Task AddLoginAsync(AppUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      if (login == null)
        throw new ArgumentNullException(nameof (login));
      
      user.Logins.Add(CreateUserLogin(user, login));

      return FlushChangesAsync(cancellationToken);
    }

    public async Task RemoveLoginAsync(AppUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      
      var login = user.Logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
      if (login != null)
      {
        user.Logins.Remove(login);
        await _session.DeleteAsync(login, cancellationToken).ConfigureAwait(false);
      }
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(AppUser user, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      var result = user.Logins
        .Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
        .ToList();
      
      return Task.FromResult<IList<UserLoginInfo>>(result);
    }

    public Task<AppUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      
      return Users.FirstOrDefaultAsync(x => x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken);
    }

    public Task<AppUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      return Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<IList<AppUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (claim == null)
        throw new ArgumentNullException(nameof (claim));
      
      var result = await Users.Where(x => x.Claims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value))
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);
      
      return result;
    }

    public async Task<IList<AppUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (string.IsNullOrEmpty(normalizedRoleName))
        throw new ArgumentNullException(normalizedRoleName);
      AppRole role = await FindRoleAsync(normalizedRoleName, cancellationToken);
      if (role == null)
        return new List<AppUser>();
      
      var result = await Users.Where(x => x.Roles.Contains(role))
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);
      
      return result;
    }

    public async Task SetTokenAsync(AppUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      ThrowIfDisposed();
      if (user == null)
        throw new ArgumentNullException(nameof (user));
      var token = user.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);

      if (token == null)
        CreateUserToken(user, loginProvider, name, value);
      else
        token.Value = value;

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
  }
