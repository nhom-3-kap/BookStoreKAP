using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
namespace BookStoreKAP.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        public AuthController(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }


        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl, string remoteError)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("/Auth/Login")]
        public async Task<IActionResult> Index(string Service, string returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Service = "Login";
            ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return View();
        }

        [Route("/Auth/Login")]
        [HttpPost]
        public async Task<IActionResult> Index(ReqLoginDTO req, string returnUrl = "~/")
        {
            returnUrl ??= Url.Content("~/");
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception();
                }

                var result = await _signInManager.PasswordSignInAsync(req.UserName, req.Password, false, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    throw new Exception();
                }

                return Redirect(returnUrl);
            }
            catch (Exception)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Service = "Login";
                ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                return RedirectToAction(nameof(Index));
            }
        }

        // Action method for /Auth/Register
        public IActionResult Register()
        {
            ViewBag.Service = "Register";
            return View();
        }

        // Action method for /Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            ViewBag.Service = "Forgot Password";
            return View();
        }

        public async Task<IActionResult> Logout(string returnUrl)
        {
            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("Home");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
