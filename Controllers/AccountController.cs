using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureWebAppDemo.Models;
using SecureWebAppDemo.ViewModel;

namespace SecureWebAppDemo.Controllers
{

    public class AccountController
        : Controller
    {

        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        public AccountController(SignInManager<AppUser> signInManager,
                                 UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        /// <summary>
        ///  returnUrl Yetkisiz erişim sonrası geri yönlendirme..
        /// </summary>
        /// <param name="loginModel"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]

        public async Task<IActionResult> Login(LoginModel loginModel, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(loginModel);
            }

            returnUrl ??= Url.Content("~/");

            var result = await _signInManager.PasswordSignInAsync(
                loginModel.email,
                loginModel.password,
                loginModel.rememberMe,
                lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction("Verify2FA", new { ReturnUrl = returnUrl });
            }

            if (result.IsLockedOut)
            {
                return View("LockOut");
            }

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View(loginModel);
        } 



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        // Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return RedirectToAction("Login", "Account");
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Code + " - " + item.Description);
                }
            }


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken] //CSRF korur
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Burada kod doğrulanır ,Doğruysa TwoFactorEnabled=true olur..
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm2FA(string code)
        {
            var user = await _userManager.GetUserAsync(User);

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user,
                                                                      _userManager.Options.Tokens.AuthenticatorTokenProvider,
                                                                      code);

            if (!isValid)
            {
                ModelState.AddModelError("", "Geçersiz Kod");
                return View("Enable2FA");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            var userItem = await _userManager.GetUserAsync(User);

            await _userManager.ResetAuthenticatorKeyAsync(userItem);

            var key = await _userManager.GetAuthenticatorKeyAsync(userItem);
            var email = await _userManager.GetEmailAsync(userItem);

            var authenticatorUri = GenerateQrCodeUri(email, key);

            ViewBag.SharedKey = key;
            ViewBag.AuthenticatorUri = authenticatorUri;

            return View();

        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Disable2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.SetTwoFactorEnabledAsync(user, false);

             
            await _userManager.ResetAuthenticatorKeyAsync(user); //token tablosunda ki authenticator key i siler.

            return RedirectToAction("Manage");
        }

        [HttpGet]
        [AllowAnonymous]

        public async Task<IActionResult> Verify2FA(string? returnUrl)
        {
            return View(new Verify2FAModel()
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FA(Verify2FAModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, false, false);

            if (result.Succeeded)
                return LocalRedirect(model.ReturnUrl ?? "~/");

            if (result.IsLockedOut)
                return View("LockOut");

            ModelState.AddModelError("", "Geçersiz Kod");
            return View(model);
        }
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return $"otpauth://totp/SecureWebAppDemo:{email}?secret={unformattedKey}&issuer=SecureWebAppDemo&digits=6";
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new ManageViewModel
            {
                Email = user.Email,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
            };

            return View(model);
        }

    }
}
