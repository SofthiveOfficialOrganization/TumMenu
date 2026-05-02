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
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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

        public async Task OnGetAsync(
            [FromQuery(Name = "DonusUrl")] string donusUrl = null,
            string returnUrl = null)
        {
            if(!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = NormalizeReturnUrl(donusUrl ?? returnUrl);

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(
            [FromForm(Name = "DonusUrl")] string donusUrl = null,
            string returnUrl = null)
        {
            returnUrl = NormalizeReturnUrl(donusUrl ?? returnUrl);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;

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

                    if (IsMeaningfulLocalReturnUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    if (roles.Contains("Admin") || roles.Contains("Owner"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                    return LocalRedirect(returnUrl);
                }

                if(result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
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

        private string NormalizeReturnUrl(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        }

        private static bool IsMeaningfulLocalReturnUrl(string returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl)
                && returnUrl != "/"
                && !returnUrl.StartsWith("/giris", StringComparison.OrdinalIgnoreCase);
        }

    }
}
