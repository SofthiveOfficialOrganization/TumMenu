using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailSender _emailSender;

        public TestEmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail([Required] string toEmail)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Lütfen geçerli bir email adresi girin";
                return View("Index");
            }

            try
            {
                var subject = "Tüm Menü - SMTP Test";
                var htmlMessage = $@"
                    <h2>Tüm Menü SMTP Test</h2>
                    <p>Email gönderme başarılı!</p>
                    <p><strong>Tarih:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    <p><strong>Hedef:</strong> {toEmail}</p>
                    <hr>
                    <p>Bu email Tüm Menü platformundan gönderilmiştir.</p>
                    <br>
                    <img src='https://tummenu.com/images/tum-menu-logo-safe-3.png' alt='Tüm Menü' style='max-width:150px;'>
                ";

                await _emailSender.SendEmailAsync(toEmail, subject, htmlMessage);

                ViewBag.Success = $"Email başarıyla gönderildi: {toEmail}";
                ViewBag.TestEmail = toEmail;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Email gönderilemedi: {ex.Message}";
                ViewBag.ErrorDetails = ex.ToString();
            }

            return View("Index");
        }
    }

    public class TestEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string ToEmail { get; set; } = string.Empty;
    }
}
