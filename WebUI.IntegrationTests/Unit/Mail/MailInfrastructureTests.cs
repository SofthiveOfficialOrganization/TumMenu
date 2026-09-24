using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
using WebUI.ExternalServices;
using WebUI.Models.Mail;

namespace WebUI.IntegrationTests.Unit.Mail;

public sealed class MailInfrastructureTests
{
    [Fact]
    public void Sanitizer_RemovesActiveContentAndUnsafeUrls()
    {
        var sanitizer = new EmailHtmlSanitizer();

        var result = sanitizer.Sanitize(
            "<p>Merhaba</p><script>alert(1)</script><a href='javascript:alert(1)' onclick='alert(2)'>Tıkla</a><img src='https://example.com/a.png' onerror='x'>");

        result.Should().Contain("Merhaba");
        result.ToLowerInvariant().Should().NotContain("script");
        result.ToLowerInvariant().Should().NotContain("javascript:");
        result.ToLowerInvariant().Should().NotContain("onclick");
        result.ToLowerInvariant().Should().NotContain("onerror");
        result.Should().Contain("https://example.com/a.png");
    }

    [Fact]
    public async Task Transport_BuildsMultipartMessageWithPlainTextAlternativeAndAttachment()
    {
        var transport = new MailTransport(Options.Create(new EmailSettings
        {
            Username = "mailer@example.com",
            Password = "secret",
            FromEmail = "mailer@example.com",
            FromName = "TumMenu"
        }));

        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("test attachment"));
        var attachment = new FormFile(content, 0, content.Length, "Attachments", "report.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var message = await transport.CreateMessageAsync(new MailComposeRequest
        {
            To = "dest@example.com",
            Subject = "Test",
            BodyHtml = "<p>Merhaba <strong>dünya</strong></p>",
            Attachments = [attachment]
        });

        message.To.Mailboxes.Single().Address.Should().Be("dest@example.com");
        message.Subject.Should().Be("Test");
        message.HtmlBody.Should().Contain("<strong>dünya</strong>");
        message.TextBody.Should().Contain("Merhaba dünya");
        message.Attachments.Should().ContainSingle();
        message.Attachments.OfType<MimePart>().Single().FileName.Should().Be("report.txt");
    }

    [Fact]
    public async Task Transport_RejectsPlaceholderConfigurationWithoutOpeningConnection()
    {
        var transport = new MailTransport(Options.Create(new EmailSettings
        {
            SmtpServer = "OVERRIDE_IN_WEBCONFIG",
            Username = "OVERRIDE_IN_WEBCONFIG",
            Password = "OVERRIDE_IN_WEBCONFIG",
            FromEmail = "OVERRIDE_IN_WEBCONFIG"
        }));

        var message = await transport.CreateMessageAsync(
            "dest@example.com",
            "Test",
            "<p>Test</p>");

        var action = () => transport.SendAsync(message, CancellationToken.None);

        await action.Should().ThrowAsync<MailboxUnavailableException>()
            .WithMessage("SMTP ayarları eksik.");
    }
}
