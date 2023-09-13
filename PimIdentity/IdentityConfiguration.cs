// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-10-23
// Comment  
// **********************************************************************************************/

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Models;
using JetBrains.Annotations;
using Pim.CommonLib;
using PimIdentity.Models;

namespace PimIdentity;

public interface IIdentityConfiguration
{
    /// <summary>
    ///     Create default roles and users if they do not exist - initialize identity store so that PIM can work with it.
    /// </summary>
    /// <returns></returns>
    Task EnsureDefaultUsersAndRolesAsync();

    /// <summary>
    ///     Set new password directly, overriding current.
    /// </summary>
    /// <param name="userName">
    ///     <see cref="ApplicationUser.UserName"/>, mandatory
    /// </param>
    /// <param name="newPassword">
    ///     Mandatory, no complexity checks.
    /// </param>
    Task ResetPasswordAsync(string userName, string newPassword);
}

/// <summary>
///     Sets up PIM security - creates required roles and default users
/// </summary>
public class IdentityConfiguration : IIdentityConfiguration
{
    public const string AdminRoleName = "Admin";
    public const string AdminUserName = "admin";
    public const string DefaultAdminPassword = "password";
    private const string ReaderRoleName = "Reader";
    private const string WriterRoleName = "Writer";

    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationUserManager _userManager;
    private readonly IdentityDatabaseContextFactory _contextFactory;

    /// <inheritdoc />
    public IdentityConfiguration([NotNull] IdentityDatabaseContextFactory databaseContextFactory)
    {
        _contextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
        _roleManager = ApplicationRoleManager.CreateOutOfContext(databaseContextFactory);
        _userManager = ApplicationUserManager.CreateOutOfContext(databaseContextFactory);
    }

    /// <inheritdoc />
    public async Task ResetPasswordAsync(string userName, string newPassword)
    {
        Check.DoRequireArgumentNotBlank(userName, nameof(userName));
        Check.DoRequireArgumentNotBlank(newPassword, nameof(newPassword));

        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
            throw new ArgumentException($"User with name '{userName}' does not exist.");

        await _userManager.RemovePasswordAsync(user);
        await _userManager.AddPasswordAsync(user, newPassword);
    }

    public async Task EnsureDefaultUsersAndRolesAsync()
    {
#pragma warning disable 4014
        EnsureRoleAsync(ReaderRoleName);
        EnsureRoleAsync(WriterRoleName);
#pragma warning restore 4014
        await EnsureRoleAsync(AdminRoleName);

        var admin = await EnsureUserAsync(AdminUserName, "admin@megapatam.com", DefaultAdminPassword);

        await _userManager.AddToRoleAsync(admin, AdminRoleName);
    }

    private async Task EnsureRoleAsync(string name)
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(name));

        if (!await _roleManager.RoleExistsAsync(name))
            await _roleManager.CreateAsync(new IdentityRole(name));
    }

    private async Task<ApplicationUser> EnsureUserAsync(string name, string email, string password)
    {
        Contract.Requires(!string.IsNullOrWhiteSpace(name));
        Contract.Requires(!string.IsNullOrEmpty(email));
        Contract.Requires(password != null);

        var user = await _userManager.FindByNameAsync(name);

        if (user == null)
        {
            user = new ApplicationUser {UserName = name, Email = email};
            await _userManager.CreateAsync(user, password);
        }

        return user;
    }
}