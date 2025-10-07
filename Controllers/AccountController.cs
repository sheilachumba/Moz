using System.Text;
using ClientPortal.Models;
using ClientPortal.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace ClientPortal.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IEmailSenderEx _email;

    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, IEmailSenderEx email)
    {
        _users = users;
        _signIn = signIn;
        _email = email;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm { AccountType = AccountType.Individual });

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (!IsStrongPassword(vm.Password))
        {
            ModelState.AddModelError(nameof(vm.Password), "Password must be at least 12 characters and include upper, lower, number and symbol.");
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            PhoneNumber = vm.Phone,
            AccountType = vm.AccountType,
            KycType = vm.AccountType switch
            {
                AccountType.Individual => KycType.Individual,
                AccountType.Company => KycType.Company,
                AccountType.SoleProprietor => KycType.SoleProprietor,
                AccountType.Association => KycType.Association,
                _ => KycType.Individual
            },
            FirstName = vm.AccountType == AccountType.Individual ? vm.FirstName : null,
            MiddleName = vm.AccountType == AccountType.Individual ? vm.MiddleName : null,
            LastName = vm.AccountType == AccountType.Individual ? vm.LastName : null,
            CompanyName = vm.AccountType == AccountType.Company ? vm.CompanyName :
                          vm.AccountType == AccountType.SoleProprietor ? vm.SoleProprietorName :
                          vm.AccountType == AccountType.Association ? vm.AssociationName : null
        };

        var res = await _users.CreateAsync(user, vm.Password);
        if (!res.Succeeded)
        {
            foreach (var e in res.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        // Compose personalized display name
        string displayName;
        if (user.AccountType == AccountType.Individual)
        {
            displayName = $"{user.FirstName} {user.MiddleName} {user.LastName}".Replace("  ", " ").Trim();
        }
        else
        {
            displayName = user.CompanyName ?? "Valued Client";
        }

        // Generate email confirmation link
        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        var encToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = encToken }, protocol: Request.Scheme);

        // Send personalized email
        await _email.SendAsync(user.Email, "Verify Your Email – Standard Bank Insurance",
            $@"<p>Dear {displayName},</p>
<p>Thank you for choosing Standard Bank Insurance as your trusted protection partner.</p>
<p>To complete your registration and secure your account, please verify your email address by clicking <a href='{link}'>here</a>.</p>
<p>Once verified, you'll have immediate access to explore our comprehensive insurance products tailored to your needs.</p>
<p>Should you need any assistance, our dedicated support team is always ready to help.</p>
<p>Sincerely,<br/>The Standard Bank Insurance Team</p>");

        return View("RegisterCheckEmail");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return Content("Verification failed.");

        var user = await _users.FindByIdAsync(userId);
        if (user == null) return Content("Verification failed.");

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch
        {
            return Content("Verification failed.");
        }

        var res = await _users.ConfirmEmailAsync(user, decodedToken);
        if (res.Succeeded)
        {
            return RedirectToAction(nameof(Login));
        }

        return Content("Verification failed.");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByEmailAsync(vm.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid login.");
            return View(vm);
        }

        var res = await _signIn.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);
        if (!res.Succeeded)
        {
            ModelState.AddModelError("", "Invalid login.");
            return View(vm);
        }

        // Redirect after login - follow same logic as DashboardController
        if (user.KycStatus == KycStatus.Submitted || user.KycStatus == KycStatus.Verified || user.KycCompleted)
        {
            // KYC submitted/completed - go to specific dashboard
            if (user.AccountType == AccountType.Individual || user.AccountType == AccountType.SoleProprietor)
                return RedirectToAction("Individual", "Dashboard");
            else if (user.AccountType == AccountType.Company || user.AccountType == AccountType.Association)
                return RedirectToAction("Company", "Dashboard");
        }

        // KYC not submitted - go to general dashboard
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByEmailAsync(vm.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return View("ForgotPasswordConfirmation");
        }

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetLink = Url.Action("ResetPassword", "Account", new { email = vm.Email, token = encodedToken }, protocol: Request.Scheme);

        await _email.SendAsync(vm.Email, "Reset Your Password - Standard Bank Insurance",
            $"<p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>");

        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return Content("Invalid reset password request.");
        }

        var vm = new ResetPasswordVm
        {
            Email = email,
            Token = token
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByEmailAsync(vm.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(vm.Token));
        var result = await _users.ResetPasswordAsync(user, decodedToken, vm.NewPassword);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(vm);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    private static bool IsStrongPassword(string pwd)
    {
        if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 12) return false;
        bool hasUpper = pwd.Any(char.IsUpper);
        bool hasLower = pwd.Any(char.IsLower);
        bool hasDigit = pwd.Any(char.IsDigit);
        bool hasSym = pwd.Any(ch => !char.IsLetterOrDigit(ch));
        return hasUpper && hasLower && hasDigit && hasSym;
    }
}