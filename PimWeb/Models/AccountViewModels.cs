using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Raven.Identity;

namespace PimWeb.Models;

public class ExternalLoginConfirmationViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; }
}

public class ExternalLoginListViewModel
{
    public string ReturnUrl { get; set; }
}

public class SendCodeViewModel
{
    public string SelectedProvider { get; set; }
    public ICollection<SelectListItem> Providers { get; set; }
    public string ReturnUrl { get; set; }
    public bool RememberMe { get; set; }
}

public class VerifyCodeViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required]
    [Display(Name = "Code")]
    public string Code { get; set; }
    public string ReturnUrl { get; set; }

    [Display(Name = "Remember this browser?")]
    public bool RememberBrowser { get; set; }

    public bool RememberMe { get; set; }
}

public class ForgotViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; }
}

public class LoginViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    public string Code { get; set; }
}

public class ManageUserViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string OldPassword { get; set; }

    [Required]
    [StringLength(100, ErrorMessage =
        "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage =
        "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }
}

public class EditUserViewModel
{
    public EditUserViewModel() { }

    // Allow Initialization with an instance of ApplicationUser:
    public EditUserViewModel(IdentityUser user)
    {
        Name = user.UserName;
        Email = user.Email;
        Id = user.Id;
    }

    public string Id { get; set; }

    [Required]
    [Display(Name = "User Name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}


public class SelectUserRolesViewModel
{
    public SelectUserRolesViewModel()
    {
        Roles = new List<SelectRoleEditorViewModel>();
    }


    // Enable initialization with an instance of ApplicationUser:
    public SelectUserRolesViewModel(IdentityUser user, IEnumerable<string> allRoles)
        : this()
    {
        UserName = user.UserName;
        Email = user.Email;
        UserId = user.Id;

        // Add all available roles to the list of EditorViewModels:
        //var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();

        allRoles
            .Select(r => new SelectRoleEditorViewModel(r) {Selected = user.Roles.Contains(r)})
            .ToList();
    }

    public string UserId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public List<SelectRoleEditorViewModel> Roles { get; set; }
}

// Used to display a single role with a checkbox, within a list structure:
public class SelectRoleEditorViewModel
{
    public SelectRoleEditorViewModel() { }

    public SelectRoleEditorViewModel(string name)
    {
        RoleName = name ?? throw new ArgumentNullException(nameof(name));
    }

    public bool Selected { get; set; }

    [Required]
    public string RoleName { get; set; }
}