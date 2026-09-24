using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace WebUI.ExternalServices;

public sealed class EmailSender(
    MailTransport transport,
    ILogger<EmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = await transport.CreateMessageAsync(email, subject, htmlMessage, CancellationToken.None);
        await transport.SendAsync(message, CancellationToken.None);

        try
        {
            await transport.AppendToSentAsync(message, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "E-posta gönderildi fakat Sent klasörüne kaydedilemedi.");
        }
    }
}
