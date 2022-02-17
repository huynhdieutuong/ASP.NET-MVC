using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AppMVC.Areas.Identity.Models.Account;
using AppMVC.ExtendMethods;
using AppMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AppMVC.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/register")]
        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Created new user.");

                    // Send mail to confirm
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.ActionLink(
                        action: nameof(ConfirmEmail),
                        values: new
                        {
                            area = "Identity",
                            userId = user.Id,
                            code = code
                        },
                        protocol: Request.Scheme
                    );

                    await _emailSender.SendEmailAsync(model.Email, "Confirm email",
                        @$"Register account successfully,
                        click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>here</a> to active your account."
                    );

                    // Redirect
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return LocalRedirect(Url.Action(nameof(RegisterConfirmation)));
                    }
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(result);
            }
            return View();
        }

        [HttpGet("/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("ConfirmEmailError");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("ConfirmEmailError");
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "ConfirmEmailError");
        }

        [HttpGet("/register-confirmation")]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet("/login")]
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(model.UserNameOrEmail);

                    if (user == null)
                    {
                        ModelState.AddModelError("User not found");
                        return View();
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogInformation("Account have been locked out.");
                    return View("Lockout");
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogInformation("User cannot sign in without a confirmed email.");
                    return View("RegisterConfirmation");
                }
                ModelState.AddModelError("Wrong password");
                return View();
            }
            return View();
        }

        [HttpPost("/logout")]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ViewData["ReturnUrl"] = returnUrl;

            _logger.LogInformation("User logged out.");
            await _signInManager.SignOutAsync();
            return LocalRedirect(returnUrl);
        }

        [HttpGet("/access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}