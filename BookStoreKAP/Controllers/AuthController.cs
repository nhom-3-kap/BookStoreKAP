using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
namespace BookStoreKAP.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly BookStoreKAPDBContext _context;
        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<Role> roleManager, BookStoreKAPDBContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl) // chuyển hướng đăng nhập
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl, string remoteError)
        {
            returnUrl ??= Url.Content("~/");
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
                // Lấy thông tin người dùng
                var userExists = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (userExists != null)
                {
                    // Tạo danh sách các claims bổ sung
                    var additionalClaims = new List<Claim>
                    {
                        new Claim("Avatar", userExists.Avatar ?? "https://placehold.co/200x200") // Thêm claim Avatar
                    };

                    // Tạo ClaimsPrincipal mới
                    var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(userExists);
                    var identity = claimsPrincipal.Identity as ClaimsIdentity;
                    identity?.AddClaims(additionalClaims);

                    // Đăng nhập người dùng với ClaimsPrincipal mới
                    await _signInManager.SignOutAsync(); // Đảm bảo đăng xuất người dùng trước khi đăng nhập lại với claims mới
                    await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal); // Đăng nhập với claims mới
                }

                return LocalRedirect(returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = new User { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                createResult = await _userManager.AddLoginAsync(user, info);
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Login");
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
                // Lấy thông tin người dùng
                var user = await _userManager.FindByNameAsync(req.UserName);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                // Tạo danh sách các claims bổ sung
                var additionalClaims = new List<Claim>
                {
                    new("Avatar", user.Avatar ?? "https://placehold.co/200x200") // Thêm claim Avatar
                };

                // Tạo ClaimsPrincipal mới
                var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
                var identity = claimsPrincipal.Identity as ClaimsIdentity;
                identity?.AddClaims(additionalClaims);

                // Đăng nhập người dùng với ClaimsPrincipal mới
                await _signInManager.SignOutAsync(); // Đảm bảo đăng xuất người dùng trước khi đăng nhập lại với claims mới
                await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal); // Đăng nhập với claims mới
                return Redirect(returnUrl);
            }
            catch (Exception)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Service = "Login";
                ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                TempData[ToastrConstant.ERROR_MSG] = "Username and password is valid!";
                return RedirectToAction(nameof(Index));
            }
        }

        // Action method for /Auth/Register
        public async Task<IActionResult> Register(string returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Service = "Login";
            ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(ReqRegisterDTO req, string returnUrl = "/")
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");
                ViewBag.Service = "Login";
                ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                return View(req);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User()
                {
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Email = req.Email,
                    PhoneNumber = req.PhoneNumber,
                    UserName = req.Username,
                    Avatar = "https://placehold.co/200x200",
                };

                var result = await _userManager.CreateAsync(user, req.Password);

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    throw new Exception("User creation failed.");
                }

                var userAfterCreated = await _userManager.FindByEmailAsync(req.Email);
                if (userAfterCreated == null)
                {
                    throw new Exception("User not found after creation.");
                }

                await transaction.CommitAsync();

                await _signInManager.PasswordSignInAsync(req.Username, req.Password, false, lockoutOnFailure: false);
                TempData[ToastrConstant.SUCCESS_MSG] = "Registered successfully";
                return Redirect(RouteConstant.HOME);
            }
            catch (Exception ex)
            {
                ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/");
                ViewBag.Service = "Login";
                ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                TempData[ToastrConstant.ERROR_MSG] = ex.Message;
                return View(req);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
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
                return Redirect("/");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
