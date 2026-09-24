using System.Net;
using System.Text.RegularExpressions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using WebUI.Models.Mail;

namespace WebUI.ExternalServices;

public sealed class MailTransport(IOptions<EmailSettings> options)
{
    private readonly EmailSettings settings = options.Value;

    public Task<MimeMessage> CreateMessageAsync(
        string to,
        string subject,
        string htmlMessage,
        CancellationToken ct = default)
    {
        return CreateMessageAsync(new MailComposeRequest
        {
            To = to,
            Subject = subject,
            BodyHtml = htmlMessage
        }, ct);
    }

    public async Task<MimeMessage> CreateMessageAsync(MailComposeRequest request, CancellationToken ct = default)
    {
        var message = new MimeMessage
        {
            MessageId = MimeUtils.GenerateMessageId(),
            Date = DateTimeOffset.UtcNow,
            Subject = request.Subject.Trim()
        };

        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        AddAddresses(message.To, request.To, "To");
        AddAddresses(message.Cc, request.Cc, "Cc");
        AddAddresses(message.Bcc, request.Bcc, "Bcc");

        if (message.To.Count == 0 && message.Cc.Count == 0 && message.Bcc.Count == 0)
        {
            throw new MailValidationException("En az bir alıcı girilmelidir.");
        }

        message.ReplyTo.Add(new MailboxAddress(settings.FromName, settings.FromEmail));

        if (!string.IsNullOrWhiteSpace(request.InReplyTo))
        {
            message.InReplyTo = request.InReplyTo.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.References))
        {
            message.Headers.Replace(HeaderId.References, request.References.Trim());
        }

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.BodyHtml,
            TextBody = HtmlToPlainText(request.BodyHtml)
        };

        foreach (var attachment in request.Attachments)
        {
            ct.ThrowIfCancellationRequested();

            if (attachment.Length <= 0)
            {
                continue;
            }

            if (attachment.Length > settings.MaxAttachmentBytes)
            {
                throw new MailValidationException(
                    $"'{attachment.FileName}' dosyası izin verilen boyutu aşıyor.");
            }

            await using var source = attachment.OpenReadStream();
            await using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer, ct);

            var contentType = string.IsNullOrWhiteSpace(attachment.ContentType)
                ? "application/octet-stream"
                : attachment.ContentType;

            bodyBuilder.Attachments.Add(
                SanitizeFileName(attachment.FileName),
                buffer.ToArray(),
                ContentType.Parse(contentType));
        }

        message.Body = bodyBuilder.ToMessageBody();
        message.Headers.Replace(HeaderId.XMailer, "TumMenu");
        message.Headers.Replace(HeaderId.XPriority, "3");
        message.Headers.Replace("List-Unsubscribe", $"<mailto:{settings.FromEmail}?subject=Unsubscribe>");
        message.Headers.Replace("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");

        return message;
    }

    public async Task SendAsync(MimeMessage message, CancellationToken ct)
    {
        EnsureSmtpConfiguration();

        using var client = new SmtpClient
        {
            Timeout = TimeoutMilliseconds()
        };

        await client.ConnectAsync(settings.SmtpServer, settings.Port, GetSmtpSocketOptions(), ct);
        await client.AuthenticateAsync(settings.Username, settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    public async Task AppendToSentAsync(MimeMessage message, CancellationToken ct)
    {
        EnsureImapConfiguration();

        using var client = new ImapClient
        {
            Timeout = TimeoutMilliseconds()
        };

        await client.ConnectAsync(settings.ImapServer, settings.ImapPort, GetImapSocketOptions(), ct);
        await client.AuthenticateAsync(settings.EffectiveImapUsername, settings.EffectiveImapPassword, ct);

        var sentFolder = await FindSentFolderAsync(client, ct)
            ?? throw new MailboxUnavailableException("Gönderilenler klasörü bulunamadı.");

        await sentFolder.OpenAsync(FolderAccess.ReadWrite, ct);
        await sentFolder.AppendAsync(message, MessageFlags.Seen, DateTimeOffset.UtcNow, ct);
        await client.DisconnectAsync(true, ct);
    }

    internal async Task<ImapClient> ConnectImapAsync(CancellationToken ct)
    {
        EnsureImapConfiguration();

        var client = new ImapClient
        {
            Timeout = TimeoutMilliseconds()
        };

        try
        {
            await client.ConnectAsync(settings.ImapServer, settings.ImapPort, GetImapSocketOptions(), ct);
            await client.AuthenticateAsync(settings.EffectiveImapUsername, settings.EffectiveImapPassword, ct);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    internal async Task<IMailFolder?> FindSentFolderAsync(ImapClient client, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(settings.SentFolder))
        {
            var configured = await FindFolderByNameAsync(client, settings.SentFolder.Trim(), ct);
            if (configured is not null)
            {
                return configured;
            }
        }

        try
        {
            var special = client.GetFolder(SpecialFolder.Sent);
            if (special?.Exists == true)
            {
                return special;
            }
        }
        catch (FolderNotFoundException)
        {
            // Fall through to the discovered-folder fallback below.
        }

        var folders = await DiscoverFoldersAsync(client, ct);
        return folders.FirstOrDefault(folder => folder.Attributes.HasFlag(FolderAttributes.Sent))
            ?? folders.FirstOrDefault(folder =>
                string.Equals(folder.Name, "Sent", StringComparison.OrdinalIgnoreCase)
                || string.Equals(folder.Name, "Sent Items", StringComparison.OrdinalIgnoreCase)
                || string.Equals(folder.Name, "Gönderilenler", StringComparison.OrdinalIgnoreCase));
    }

    internal async Task<IReadOnlyList<IMailFolder>> DiscoverFoldersAsync(ImapClient client, CancellationToken ct)
    {
        var folders = new Dictionary<string, IMailFolder>(StringComparer.OrdinalIgnoreCase);
        var inbox = client.Inbox;
        if (inbox is not null)
        {
            folders[inbox.FullName] = inbox;
        }

        foreach (var personalNamespace in client.PersonalNamespaces)
        {
            var root = client.GetFolder(personalNamespace);
            await DiscoverChildrenAsync(root, folders, ct);
        }

        return folders.Values
            .Where(folder => !folder.IsNamespace && !folder.Attributes.HasFlag(FolderAttributes.NoSelect))
            .OrderBy(folder => folder.FullName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    internal string EncodeFolderKey(string fullName) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fullName))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    internal string DecodeFolderKey(string key)
    {
        try
        {
            var value = key.Replace('-', '+').Replace('_', '/');
            value = value.PadRight(value.Length + ((4 - value.Length % 4) % 4), '=');
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch (FormatException)
        {
            throw new MailValidationException("Geçersiz mail klasörü.");
        }
    }

    private async Task<IMailFolder?> FindFolderByNameAsync(
        ImapClient client,
        string name,
        CancellationToken ct)
    {
        var folders = await DiscoverFoldersAsync(client, ct);
        return folders.FirstOrDefault(folder =>
            string.Equals(folder.FullName, name, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folder.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task DiscoverChildrenAsync(
        IMailFolder parent,
        IDictionary<string, IMailFolder> folders,
        CancellationToken ct)
    {
        if (!parent.Attributes.HasFlag(FolderAttributes.HasChildren))
        {
            return;
        }

        var children = await parent.GetSubfoldersAsync(false, ct);
        foreach (var child in children)
        {
            folders.TryAdd(child.FullName, child);
            await DiscoverChildrenAsync(child, folders, ct);
        }
    }

    private void EnsureSmtpConfiguration()
    {
        if (IsMissing(settings.SmtpServer)
            || IsMissing(settings.Username)
            || IsMissing(settings.Password)
            || IsMissing(settings.FromEmail))
        {
            throw new MailboxUnavailableException("SMTP ayarları eksik.");
        }
    }

    private void EnsureImapConfiguration()
    {
        if (IsMissing(settings.ImapServer)
            || IsMissing(settings.EffectiveImapUsername)
            || IsMissing(settings.EffectiveImapPassword))
        {
            throw new MailboxUnavailableException("IMAP ayarları eksik.");
        }
    }

    private int TimeoutMilliseconds() => Math.Clamp(settings.ConnectionTimeoutSeconds, 5, 120) * 1000;

    private static bool IsMissing(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || value.Contains("OVERRIDE_IN_", StringComparison.OrdinalIgnoreCase);

    private SecureSocketOptions GetSmtpSocketOptions()
    {
        if (!settings.EnableSsl)
        {
            return SecureSocketOptions.None;
        }

        return settings.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }

    private SecureSocketOptions GetImapSocketOptions()
    {
        if (!settings.ImapEnableSsl)
        {
            return SecureSocketOptions.None;
        }

        return settings.ImapPort == 993
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }

    private static void AddAddresses(InternetAddressList target, string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        try
        {
            var parsed = InternetAddressList.Parse(value.Replace(';', ','));
            foreach (var mailbox in parsed.Mailboxes)
            {
                target.Add(mailbox);
            }
        }
        catch (ParseException ex)
        {
            throw new MailValidationException($"{fieldName} alanındaki adreslerden biri geçersiz.", ex);
        }
    }

    private static string HtmlToPlainText(string html)
    {
        var value = Regex.Replace(html ?? string.Empty, "<br\\s*/?>", Environment.NewLine, RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "</p\\s*>", Environment.NewLine + Environment.NewLine, RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<[^>]+>", string.Empty);
        return WebUtility.HtmlDecode(value).Trim();
    }

    internal static string SanitizeFileName(string fileName)
    {
        var value = Path.GetFileName(fileName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return "ek";
        }

        return Regex.Replace(value, "[^\\p{L}\\p{N}._ -]", "_");
    }
}

public sealed class MailValidationException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class MailboxUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
