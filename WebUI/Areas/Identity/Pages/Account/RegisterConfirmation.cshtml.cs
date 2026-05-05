// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WebUI.Services;
using WebUI.Models.Email;

namespace WebUI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _sender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<RegisterConfirmationModel> _logger;

        public RegisterConfirmationModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender sender,
            IEmailTemplateService emailTemplateService,
            ILogger<RegisterConfirmationModel> logger)
        {
            _userManager = userManager;
            _sender = sender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        public string Email { get; set; }
        public bool DisplayConfirmAccountLink { get; set; }
        public string EmailConfirmationUrl { get; set; }
        public bool EmailSent { get; set; } = true;
        public string StatusMessage { get; set; }

        [TempData]
        public string InfoMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null, bool emailSent = true)
        {
            if(email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            EmailSent = emailSent;

            // Check if email is already confirmed
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = "E-posta adresiniz zaten onaylanmış. Giriş yapabilirsiniz.";
            }

            // Since we're sending real emails, we don't need to display the confirmation link
            DisplayConfirmAccountLink = false;
            if(DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendEmailAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                _logger.LogWarning("Resend email attempted for non-existent user: {Email}", email);
                StatusMessage = "Eğer bu e-posta adresi sistemimizde kayıtlıysa, onay e-postası gönderilecektir.";
                Email = email;
                EmailSent = true;
                return Page();
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = "E-posta adresiniz zaten onaylanmış. Giriş yapabilirsiniz.";
                Email = email;
                EmailSent = true;
                return Page();
            }

            try
            {
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
                    UserName = email,
                    UserEmail = email,
                    ConfirmationUrl = confirmationUrl,
                    AccountActivationUrl = accountActivationUrl
                };

                var emailHtml = await _emailTemplateService.GenerateRegistrationEmail(emailModel);
                await _sender.SendEmailAsync(user.Email!, "TumMenu'a Hoş Geldiniz! - Hesabınızı Onaylayın", emailHtml);

                _logger.LogInformation("Resent confirmation email to {Email}", email);
                StatusMessage = "Onay e-postası başarıyla gönderildi. Lütfen e-posta kutunuzu kontrol edin.";
                EmailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend confirmation email to {Email}", email);
                StatusMessage = "E-posta gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin veya destek ekibiyle iletişime geçin.";
                EmailSent = false;
            }

            Email = email;
            return Page();
        }
    }
}
