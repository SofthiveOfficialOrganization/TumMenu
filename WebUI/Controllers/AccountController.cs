using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WebUI.Models.Email;
using Domain.Entities;

namespace WebUI.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ActivateAccount(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Kullanıcı bulunamadı. Lütfen kayıt olduğunuz e-posta adresini kontrol edin.";
                return View();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            // Check if email is already confirmed
            if (!user.EmailConfirmed)
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);
                
                if (!result.Succeeded)
                {
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Hesabınız açılırken bir hata oluştu. Lütfen daha sonra tekrar deneyin veya destek ekibimizle iletişime geçin.";
                    
                    _logger.LogError($"Failed to confirm email for user {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return View();
                }
            }

            // Auto sign in after successful activation
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            ViewBag.UserName = user.Email;
            ViewBag.IsSuccess = true;
            ViewBag.Message = "Hesabınız başarıyla açıldı! TumMenu ailesine hoş geldiniz.";
            
            _logger.LogInformation($"User {user.Email} successfully activated their account.");
            return View();
        }

        [HttpGet]
        public IActionResult AccountActivationSuccess()
        {
            return View();
        }
    }
}
