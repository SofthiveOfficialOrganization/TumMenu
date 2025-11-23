using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebUI.ExternalServices
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine("EMAİLİNİZ GELDİİİİİİİ", htmlMessage);
            return Task.CompletedTask;
        }
    }
}
