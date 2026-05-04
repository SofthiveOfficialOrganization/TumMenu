// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using WebUI.Services.Turnstile;

namespace WebUI.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITurnstileService _turnstileService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager, ITurnstileService turnstileService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _turnstileService = turnstileService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     Oturum süresi dolduğunda gösterilecek mesaj
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Oturum sona erdi bilgisi
        /// </summary>
        [TempData]
        public string SessionExpired { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "E-posta adresi gereklidir.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Şifre gereklidir.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Beni Hatırla")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync()
        {
            if(!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Oturum sona erdi mesajını TempData'dan al ve ModelState'e ekle
            var tempDataFactory = HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
            if (tempDataFactory != null)
            {
                var tempData = tempDataFactory.GetTempData(HttpContext);
                if (tempData["SessionExpired"] != null)
                {
                    ModelState.AddModelError(string.Empty, tempData["SessionExpired"].ToString());
                }
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Validate Turnstile token
            var turnstileToken = Request.Form["cf-turnstile-response"];
            var turnstileResult = await _turnstileService.ValidateAsync(turnstileToken);
            if (!turnstileResult.Success)
            {
                ModelState.AddModelError(string.Empty, "İnsan doğrulaması başarısız. Lütfen tekrar deneyin.");
                return Page();
            }

            if(ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if(result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    ApplicationUser user = await _userManager.FindByNameAsync(Input.Email);
                    if(user is null)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                        return Page();
                    }

                    var roles = await _userManager.GetRolesAsync(user);
                    var allowed = roles.Contains("Owner") || roles.Contains("Admin");

                    if(!allowed)
                    {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Bu hesap ile giriş yapılamaz.");
                        return Page();
                    }

                    user.LastLogin = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    await _userManager.UpdateAsync(user);

                    // Her zaman Admin Dashboard'a yönlendir - ReturnUrl artık kullanılmıyor
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                if(result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { RememberMe = Input.RememberMe });
                }
                if(result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "E-posta adresi veya şifre hatalı.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

    }
}
