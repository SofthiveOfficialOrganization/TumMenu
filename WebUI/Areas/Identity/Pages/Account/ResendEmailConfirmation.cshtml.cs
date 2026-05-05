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
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using WebUI.Models.Email;
using WebUI.Services;

namespace WebUI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<ResendEmailConfirmationModel> _logger;

        public ResendEmailConfirmationModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<ResendEmailConfirmationModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "E-posta adresi gereklidir.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if(user == null)
            {
                // Don't reveal that the user does not exist
                _logger.LogInformation("Resend email attempted for non-existent user: {Email}", Input.Email);
                StatusMessage = "Eğer bu e-posta adresi sistemimizde kayıtlıysa, onay e-postası gönderilecektir.";
                return RedirectToPage("./ResendEmailConfirmation");
            }

            // Check if email is already confirmed
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = "E-posta adresiniz zaten onaylanmış. Giriş yapabilirsiniz.";
                return RedirectToPage("./ResendEmailConfirmation");
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
                    UserName = Input.Email,
                    UserEmail = Input.Email,
                    ConfirmationUrl = confirmationUrl,
                    AccountActivationUrl = accountActivationUrl
                };

                var emailHtml = await _emailTemplateService.GenerateRegistrationEmail(emailModel);
                await _emailSender.SendEmailAsync(user.Email!, "TumMenu'a Hoş Geldiniz! - Hesabınızı Onaylayın", emailHtml);

                _logger.LogInformation("Resent confirmation email to {Email}", Input.Email);
                StatusMessage = "Onay e-postası başarıyla gönderildi. Lütfen e-posta kutunuzu kontrol edin.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend confirmation email to {Email}", Input.Email);
                StatusMessage = "E-posta gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin veya destek ekibiyle iletişime geçin.";
            }

            return RedirectToPage("./ResendEmailConfirmation");
        }
    }
}
