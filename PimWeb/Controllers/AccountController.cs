using System.Reflection;
using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pim.CommonLib;
using PimWeb.Models;


namespace PimWeb.Controllers;

[Authorize]
public class AccountController : Controller
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly SignInManager<IdentityUser<int>> _signInManager;
    private readonly UserManager<IdentityUser<int>> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public AccountController(UserManager<IdentityUser<int>> userManager, SignInManager<IdentityUser<int>> signInManager
            , RoleManager<IdentityRole<int>> roleManager
        )
    {
        Log.DebugFormat("Instantiating with provided dependencies.");
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public SignInManager<IdentityUser<int>> SignInManager => _signInManager;

    public UserManager<IdentityUser<int>> UserManager => _userManager;

    public RoleManager<IdentityRole<int>> RoleManager => _roleManager;

    //
    // GET: /Account/Login
    [AllowAnonymous]
    public ActionResult Login(string returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        
        return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, change to shouldLockout: true
        var result = await SignInManager.PasswordSignInAsync(model.Name, model.Password, model.RememberMe, false);

        Log.InfoFormat("{0} sign in result: {1}; IsAuthenticated: {2}", model.Name, result, result.Succeeded);

        if (result.Succeeded)
            return RedirectToLocal(returnUrl);

        if (result.IsLockedOut)
            ModelState.AddModelError("", "Locked out.");
        
        // unsupported in aspnetcore?
        //if (result.RequiresVerification)
        //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        
        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    //
    // GET: /Account/Register
    [AllowAnonymous]
    public ActionResult Register()
    {
        return View();
    }

    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser<int> { UserName = model.Name, Email = model.Name };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, false);
                    
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                return RedirectToAction("Index", "Home");
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    //
    // GET: /Account/ConfirmEmail
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null)
            return View("Error");
        
        var result = await UserManager.ConfirmEmailAsync(user, code);
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    //
    // GET: /Account/ForgotPassword
    [AllowAnonymous]
    public ActionResult ForgotPassword()
    {
        return View();
    }

    //
    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
            // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
            // return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    //
    // GET: /Account/ForgotPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    //
    // GET: /Account/ResetPassword
    [AllowAnonymous]
    public ActionResult ResetPassword(string code)
    {
        return code == null ? View("Error") : View();
    }

    //
    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await UserManager.FindByNameAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }
        var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }
        AddErrors(result);
        return View();
    }

    //
    // GET: /Account/ResetPasswordConfirmation
    [AllowAnonymous]
    public ActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    //
    // POST: /Account/LogOff
    [HttpPost]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> LogOff()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Index", "Home");
    }

    // client is redirected here after successful password change
    [Authorize]
    public ActionResult ChangePassword(ManageMessageId? message)
    {
        ViewBag.StatusMessage =
            message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.Error ? "An error has occurred."
            : "";
        ViewBag.ReturnUrl = Url.Action("ChangePassword");
        return View();
    }

    // change password page submits here
    [HttpPost]
    [Authorize]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> ChangePassword(ManageUserViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        var hasPassword = !user.PasswordHash.IsNullOrEmpty();
        ViewBag.HasLocalPassword = hasPassword;
        ViewBag.ReturnUrl = Url.Action("ChangePassword");
        if (hasPassword)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("ChangePassword", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
            }
        }
        else
        {
            // User does not have a password so remove any validation errors caused by a missing OldPassword field
            var state = ModelState["OldPassword"];
            state?.Errors.Clear();

            if (ModelState.IsValid)
            {
                IdentityResult result = await UserManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("ChangePassword", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult Manage()
    {
        var model = UserManager.Users.ToList().Select(u => new EditUserViewModel(u)).ToList();
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Edit(string id, ManageMessageId? message = null)
    {
        var user = await UserManager.FindByIdAsync(id);
        var model = new EditUserViewModel(user);
        ViewBag.MessageId = message;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await UserManager.FindByIdAsync(model.Id.ToString());
            user.UserName = model.Name;
            user.Email = model.Email;
            await UserManager.UpdateAsync(user);
            return RedirectToAction("Manage");
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(string id = null, string name = null)
    {
        var user = await UserManager.FindByIdAsync(id);
        var model = new EditUserViewModel(user);
        if (user == null)
        {
            return NotFound();
        }
        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(string id)
    {
        var user = await UserManager.FindByIdAsync(id);
        await UserManager.DeleteAsync(user);
        return RedirectToAction("Manage");
    }

    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UserRoles(string id, string name = null)
    {
        var user = await UserManager.FindByIdAsync(id);
        var allRoles = RoleManager.Roles.Select(x => x.Name).ToList();
        var userRoles = (await UserManager.GetRolesAsync(user)).ToCaseInsensitiveSet();
        
        var model = new SelectUserRolesViewModel(user, userRoles, allRoles);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    // under IIS this produces BadRequest
    //[ValidateAntiForgeryToken]
    public async Task<ActionResult> UserRoles(SelectUserRolesViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await UserManager.FindByIdAsync(model.UserId.ToString());
            var userRoles = await UserManager.GetRolesAsync(user);

            var currentUserRoleNames = userRoles.ToArray();
            await UserManager.RemoveFromRolesAsync(user, currentUserRoleNames);

            var newRoles = model.Roles.Where(mr => mr.Selected).Select(mr => mr.RoleName).ToArray();
            await UserManager.AddToRolesAsync(user, newRoles);

            return RedirectToAction("Manage");
        }
        return View(model);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _roleManager?.Dispose();
            _userManager?.Dispose();
        }

        base.Dispose(disposing);
    }

    // -------------- Helpers

    // Used for XSRF protection when adding external logins
    private const string XsrfKey = "XsrfId";

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    private ActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            Log.DebugFormat("Redirecting to {0}", returnUrl);
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    public enum ManageMessageId
    {
        AddPhoneSuccess,
        ChangePasswordSuccess,
        SetTwoFactorSuccess,
        SetPasswordSuccess,
        RemoveLoginSuccess,
        RemovePhoneSuccess,
        Error
    }
}