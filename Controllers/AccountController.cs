using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SecureWebAppDemo.Models;
using SecureWebAppDemo.ViewModel;
using System.Security.Claims;

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

        public async Task<IActionResult> LoginOld(LoginModel loginModel, string? returnUrl = null)
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

        /// <summary>
        ///  returnUrl Yetkisiz erişim sonrası geri yönlendirme.. Custom Claim ekleme
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
                return View(loginModel);

            returnUrl ??= Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(loginModel.email);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                return View(loginModel);
            }

            var passwordResult = await _signInManager.CheckPasswordSignInAsync(user,
                                                                               loginModel.password,
                                                                               lockoutOnFailure: true);

            if (passwordResult.IsLockedOut)
                return View("LockOut");

            if (passwordResult.RequiresTwoFactor)
                return RedirectToAction("Verify2FA", new { ReturnUrl = returnUrl });

            if (!passwordResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                return View(loginModel);
            }

            //Custom Claim Ekleme

            var additionalClaims = new List<Claim>()
            {
                 new Claim("CustomClaim1","Volkan"),
                 new Claim("CustomClaim2","Tolkan")
            };

            /* var authProps = new AuthenticationProperties()
             {
                 IsPersistent = true,
                 ExpiresUtc = DateTime.UtcNow,

             };

             authProps.Parameters.Add("CustomProp", "Samsun55");


             await _signInManager.SignInWithClaimsAsync(user,
                                              authProps,
                                              additionalClaims);*/


            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
            };

            authProps.Items["CustomProp"] = "Samsun55";

            await _signInManager.SignInWithClaimsAsync(
                user,
                authProps,
                additionalClaims);


            return LocalRedirect(returnUrl);
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
                UserName = model.Email,
                TwoFactorEnabled = true
            };

            //
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Code + " - " + item.Description);
                }

                return View(model);
            }

            // Kalıcı claim ekleme(DB -->UserClaims tablosuna ekler)
            await _userManager.AddClaimAsync(user,
                                             new System.Security.Claims.Claim("Department", "IT"));

            await _userManager.AddClaimAsync(user,
                                             new System.Security.Claims.Claim(ClaimTypes.Role, "Admin"));

            return RedirectToAction("Login", "Account");
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

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                code.Replace(" ", "").Replace("-", "")
            );

            if (!isValid)
            {
                ModelState.AddModelError("", "Geçersiz Kod");
                return View("Enable2FA");
            }

            // ✅ 2FA aktif edilir
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            return RedirectToAction("Manage");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            var user = await _userManager.GetUserAsync(User);

            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = user.Email;

            var authenticatorUri = GenerateQrCodeUri(email, key);

            ViewBag.SharedKey = key;
            ViewBag.AuthenticatorUri = authenticatorUri;
            ViewBag.QrCodeImage = GenerateQrCodeImage(authenticatorUri);
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

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model
                                                                                .Code
                                                                                .Replace(" ", "")
                                                                                .Replace("-", ""),
                                                                                model.RememberMe,
                                                                                rememberClient: true); //remember client cihazı hatırla

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

        private string GenerateQrCodeImage(string uri)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new Base64QRCode(data);
            return qrCode.GetGraphic(20);
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
