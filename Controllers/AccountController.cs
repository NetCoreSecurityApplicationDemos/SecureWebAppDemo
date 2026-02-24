using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureWebAppDemo.Models;

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
                return RedirectToAction("Verify2FA", new { returnUrl });
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
        public async Task<IActionResult> Register()
        {
            return View();
        }

        // Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return RedirectToAction("Login", "Account");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken] //CSRF korur
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
