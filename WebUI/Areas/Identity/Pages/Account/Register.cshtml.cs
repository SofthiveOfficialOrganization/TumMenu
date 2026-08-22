// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Application.Owners.Commands;
using Application.SystemSettings.DTOs;
using Application.SystemSettings.Queries;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using WebUI.Models.Email;
using WebUI.Services;
using WebUI.Services.Turnstile;

namespace WebUI.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IMediator _mediator;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITurnstileService _turnstileService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<ApplicationRole> roleManager,
            IMediator mediator,
            ApplicationDbContext db,
            IEmailTemplateService emailTemplateService,
            ITurnstileService turnstileService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _db = db;
            _mediator = mediator;
            _emailTemplateService = emailTemplateService;
            _turnstileService = turnstileService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        public LegalVersionSettingsDTO LegalVersions { get; private set; } = new();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Display(Name = "E-posta")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Şifre gereklidir.")]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Şifre Tekrar")]
            [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
            public string ConfirmPassword { get; set; }

            [Range(typeof(bool), "true", "true", ErrorMessage = "Kullanıcı sözleşmesini kabul etmelisiniz.")]
            [Display(Name = "Kullanıcı sözleşmesini kabul ediyorum")]
            public bool AcceptTerms { get; set; }

            [Range(typeof(bool), "true", "true", ErrorMessage = "KVKK aydınlatma metnini onaylamalısınız.")]
            [Display(Name = "KVKK aydınlatma metnini onaylıyorum")]
            public bool AcceptKvkkNotice { get; set; }

            [Range(typeof(bool), "true", "true", ErrorMessage = "Gizlilik politikasını okuduğunuzu onaylamalısınız.")]
            [Display(Name = "Gizlilik politikasını okudum")]
            public bool AcceptPrivacyNotice { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            await LoadLegalVersionsAsync();
        }

        public async Task<IActionResult> OnGetCheckEmailAsync(string email)
        {
            email = email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                return new JsonResult(new
                {
                    isValid = false,
                    message = "E-posta adresi gereklidir."
                });
            }

            if (!new EmailAddressAttribute().IsValid(email))
            {
                return new JsonResult(new
                {
                    isValid = false,
                    message = "Geçerli bir e-posta adresi giriniz."
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return new JsonResult(new
                {
                    isValid = false,
                    message = "Bu e-posta adresi zaten kayıtlı."
                });
            }

            return new JsonResult(new
            {
                isValid = true,
                message = "Bu e-posta adresi kullanılabilir."
            });
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null, CancellationToken ct = default)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            await LoadLegalVersionsAsync(ct);

            // Validate Turnstile token
            var turnstileToken = Request.Form["cf-turnstile-response"];
            var turnstileResult = await _turnstileService.ValidateAsync(turnstileToken);
            if (!turnstileResult.Success)
            {
                ModelState.AddModelError("Turnstile", "Human verification failed. Please try again.");
                return Page();
            }

            if(ModelState.IsValid)
            {
                var acceptedAtUtc = DateTime.UtcNow;
                var acceptedIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var acceptedUserAgent = Request.Headers.UserAgent.ToString();

                var user = CreateUser();
                user.AcceptedTermsVersion = LegalVersions.TermsVersion;
                user.AcceptedKvkkVersion = LegalVersions.KvkkVersion;
                user.AcceptedPrivacyVersion = LegalVersions.PrivacyVersion;
                user.LegalAcceptedAtUtc = acceptedAtUtc;
                user.LegalAcceptedIp = string.IsNullOrWhiteSpace(acceptedIp) ? null : acceptedIp[..Math.Min(64, acceptedIp.Length)];
                user.LegalAcceptedUserAgent = string.IsNullOrWhiteSpace(acceptedUserAgent)
                    ? null
                    : acceptedUserAgent[..Math.Min(512, acceptedUserAgent.Length)];

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);
                if(result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _mediator.Send(new CreateOwnerCommand { ApplicationUserId = user.Id }, ct);
                    const string ownerRole = "Owner";
                    if(!await _roleManager.RoleExistsAsync(ownerRole))
                        await _roleManager.CreateAsync(new ApplicationRole(ownerRole));

                    if(!await _userManager.IsInRoleAsync(user, ownerRole))
                        await _userManager.AddToRoleAsync(user, ownerRole);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    
                    var confirmationUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme);

                    var accountActivationUrl = Url.Action(
                        "ActivateAccount", 
                        "Account",
                        values: new { userId = userId, code = code },
                        protocol: Request.Scheme);

                    var emailModel = new RegistrationEmailModel
                    {
                        UserName = Input.Email,
                        UserEmail = Input.Email,
                        ConfirmationUrl = confirmationUrl,
                        AccountActivationUrl = accountActivationUrl
                    };

                    var emailHtml = await _emailTemplateService.GenerateRegistrationEmail(emailModel);

                    bool emailSent = false;
                    string emailErrorMessage = null;
                    try
                    {
                        await _emailSender.SendEmailAsync(user.Email!, "TumMenu'a Hoş Geldiniz! - Hesabınızı Onaylayın", emailHtml);
                        emailSent = true;
                        _logger.LogInformation("Registration email sent successfully to {Email}", Input.Email);
                    }
                    catch (Exception ex)
                    {
                        emailErrorMessage = ex.Message;
                        _logger.LogError(ex, "Failed to send registration email to {Email}. Account was created successfully.", Input.Email);
                    }

                    if(_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        if (!emailSent)
                        {
                            TempData["InfoMessage"] = "Hesabınız başarıyla oluşturuldu ancak onay e-postası gönderilemedi. Lütfen daha sonra tekrar deneyin veya destek ekibiyle iletişime geçin.";
                            TempData["EmailError"] = emailErrorMessage;
                        }
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl, emailSent = emailSent });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        if (!emailSent)
                        {
                            TempData["WarningMessage"] = "Hesabınız oluşturuldu ancak onay e-postası gönderilemedi. Ayarlardan e-posta adresinizi doğrulayabilirsiniz.";
                        }
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                var user = Activator.CreateInstance<ApplicationUser>();
                user.CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return user;
            }
            catch
            {
                throw new InvalidOperationException($"'{nameof(ApplicationUser)}' örneği oluşturulamıyor. " +
                    $"'{nameof(ApplicationUser)}' soyut bir sınıf olmadığından ve parametresiz bir kurucuya sahip olduğundan emin olun, veya alternatif olarak " +
                    $"kayıt sayfasını /Areas/Identity/Pages/Account/Register.cshtml içinde geçersiz kılın");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if(!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Varsayılan arayüz e-posta desteği olan bir kullanıcı deposu gerektirir.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private async Task LoadLegalVersionsAsync(CancellationToken ct = default)
        {
            LegalVersions = await _mediator.Send(new GetLegalVersionSettingsQuery(), ct);
        }
    }
}
