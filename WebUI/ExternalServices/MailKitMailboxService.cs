using System.Text.Encodings.Web;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using MimeKit;
using WebUI.Models.Mail;

namespace WebUI.ExternalServices;

public sealed class MailKitMailboxService(
    MailTransport transport,
    EmailHtmlSanitizer sanitizer,
    ILogger<MailKitMailboxService> logger) : IMailboxService
{
    private const int DefaultPageSize = 50;

    public async Task<IReadOnlyList<MailFolderDto>> GetFoldersAsync(CancellationToken ct = default)
    {
        using var client = await transport.ConnectImapAsync(ct);
        var folders = await transport.DiscoverFoldersAsync(client, ct);
        var result = new List<MailFolderDto>(folders.Count);

        foreach (var folder in folders)
        {
            try
            {
                await folder.StatusAsync(StatusItems.Count | StatusItems.Unread, ct);
                result.Add(ToFolderDto(folder));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogDebug(ex, "IMAP klasör durumu okunamadı: {Folder}", folder.FullName);
                result.Add(ToFolderDto(folder));
            }
        }

        await client.DisconnectAsync(true, ct);
        return result
            .OrderBy(folder => FolderOrder(folder))
            .ThenBy(folder => folder.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<MailMessageListResult> ListMessagesAsync(
        string folderKey,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, DefaultPageSize);

        using var client = await transport.ConnectImapAsync(ct);
        var folder = await FindFolderAsync(client, folderKey, ct);
        await folder.OpenAsync(FolderAccess.ReadOnly, ct);

        var query = BuildSearchQuery(search);
        var allUids = await folder.SearchAsync(query, ct);
        var orderedUids = allUids.OrderByDescending(uid => uid.Id).ToArray();
        var pageUids = orderedUids
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var summaries = pageUids.Length == 0
            ? []
            : await folder.FetchAsync(
                pageUids,
                MessageSummaryItems.Envelope
                | MessageSummaryItems.Flags
                | MessageSummaryItems.InternalDate
                | MessageSummaryItems.BodyStructure
                | MessageSummaryItems.PreviewText,
                ct);

        var mapped = summaries
            .OrderByDescending(summary => summary.UniqueId.Id)
            .Select(summary => ToSummary(folderKey, summary))
            .ToArray();

        await client.DisconnectAsync(true, ct);

        return new MailMessageListResult
        {
            Messages = mapped,
            TotalCount = orderedUids.Length,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MailMessageDetailDto> GetMessageAsync(
        string folderKey,
        uint uid,
        bool markAsRead = true,
        CancellationToken ct = default)
    {
        using var client = await transport.ConnectImapAsync(ct);
        var folder = await FindFolderAsync(client, folderKey, ct);
        await folder.OpenAsync(markAsRead ? FolderAccess.ReadWrite : FolderAccess.ReadOnly, ct);

        var uniqueId = new UniqueId(uid);
        var message = await folder.GetMessageAsync(uniqueId, ct);

        if (markAsRead)
        {
            await folder.AddFlagsAsync(uniqueId, MessageFlags.Seen, silent: true, ct);
        }

        var summary = await folder.FetchAsync(
            [uniqueId],
            MessageSummaryItems.Flags,
            ct);

        var isRead = markAsRead || summary.FirstOrDefault()?.Flags?.HasFlag(MessageFlags.Seen) == true;
        var detail = ToDetail(folderKey, uid, message, isRead);

        await client.DisconnectAsync(true, ct);
        return detail;
    }

    public async Task MarkReadAsync(string folderKey, uint uid, bool isRead, CancellationToken ct = default)
    {
        using var client = await transport.ConnectImapAsync(ct);
        var folder = await FindFolderAsync(client, folderKey, ct);
        await folder.OpenAsync(FolderAccess.ReadWrite, ct);

        var uniqueId = new UniqueId(uid);
        if (isRead)
        {
            await folder.AddFlagsAsync(uniqueId, MessageFlags.Seen, silent: true, ct);
        }
        else
        {
            await folder.RemoveFlagsAsync(uniqueId, MessageFlags.Seen, silent: true, ct);
        }

        await client.DisconnectAsync(true, ct);
    }

    public async Task MoveAsync(
        string sourceFolderKey,
        uint uid,
        string destinationFolderKey,
        CancellationToken ct = default)
    {
        using var client = await transport.ConnectImapAsync(ct);
        var source = await FindFolderAsync(client, sourceFolderKey, ct);
        var destination = await FindFolderAsync(client, destinationFolderKey, ct);
        await source.OpenAsync(FolderAccess.ReadWrite, ct);
        await source.MoveToAsync(new UniqueId(uid), destination, ct);
        await client.DisconnectAsync(true, ct);
    }

    public async Task DeleteAsync(string folderKey, uint uid, CancellationToken ct = default)
    {
        using var client = await transport.ConnectImapAsync(ct);
        var source = await FindFolderAsync(client, folderKey, ct);
        await source.OpenAsync(FolderAccess.ReadWrite, ct);

        var trash = await TryFindSpecialFolderAsync(client, SpecialFolder.Trash, ct);
        var uniqueId = new UniqueId(uid);

        if (trash is not null
            && !string.Equals(source.FullName, trash.FullName, StringComparison.OrdinalIgnoreCase))
        {
            await source.MoveToAsync(uniqueId, trash, ct);
        }
        else
        {
            await source.AddFlagsAsync(uniqueId, MessageFlags.Deleted, silent: true, ct);
            await source.ExpungeAsync([uniqueId], ct);
        }

        await client.DisconnectAsync(true, ct);
    }

    public async Task<MailAttachmentContent> DownloadAttachmentAsync(
        string folderKey,
        uint uid,
        string partId,
        CancellationToken ct = default)
    {
        using var client = await transport.ConnectImapAsync(ct);
        var folder = await FindFolderAsync(client, folderKey, ct);
        await folder.OpenAsync(FolderAccess.ReadOnly, ct);
        var message = await folder.GetMessageAsync(new UniqueId(uid), ct);
        var part = EnumerateMimeParts(message.Body)
            .FirstOrDefault(item => item.Id == partId)
            .Part;

        if (part is null || !IsAttachment(part))
        {
            throw new MailValidationException("Ek dosyası bulunamadı.");
        }

        if (part.Content is null)
        {
            throw new MailValidationException("Ek dosyasının içeriği alınamadı.");
        }

        await using var buffer = new MemoryStream();
        part.Content.DecodeTo(buffer);

        if (buffer.Length > 25 * 1024 * 1024)
        {
            throw new MailValidationException("Ek dosyası izin verilen boyutu aşıyor.");
        }

        buffer.Position = 0;
        var result = new MailAttachmentContent
        {
            FileName = MailTransport.SanitizeFileName(part.FileName ?? "ek"),
            ContentType = part.ContentType.MimeType,
            Content = new MemoryStream(buffer.ToArray(), writable: false)
        };

        await client.DisconnectAsync(true, ct);
        return result;
    }

    public async Task<MailSendResult> SendAsync(MailComposeRequest request, CancellationToken ct = default)
    {
        request.BodyHtml = sanitizer.Sanitize(request.BodyHtml);
        if (string.IsNullOrWhiteSpace(request.BodyHtml))
        {
            throw new MailValidationException("Mesaj gövdesi boş olamaz.");
        }

        var message = await transport.CreateMessageAsync(request, ct);
        await transport.SendAsync(message, ct);

        try
        {
            await transport.AppendToSentAsync(message, ct);
            return new MailSendResult { Sent = true, Archived = true };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Mail gönderildi fakat IMAP Sent append işlemi başarısız oldu.");
            return new MailSendResult
            {
                Sent = true,
                Archived = false,
                Warning = "Mail gönderildi; ancak Gönderilenler klasörüne kaydedilemedi."
            };
        }
    }

    private async Task<IMailFolder> FindFolderAsync(ImapClient client, string folderKey, CancellationToken ct)
    {
        var requestedName = transport.DecodeFolderKey(folderKey);
        var folders = await transport.DiscoverFoldersAsync(client, ct);
        var folder = folders.FirstOrDefault(item =>
            string.Equals(item.FullName, requestedName, StringComparison.OrdinalIgnoreCase));

        return folder ?? throw new MailValidationException("Mail klasörü bulunamadı.");
    }

    private async Task<IMailFolder?> TryFindSpecialFolderAsync(
        ImapClient client,
        SpecialFolder specialFolder,
        CancellationToken ct)
    {
        try
        {
            var folder = client.GetFolder(specialFolder);
            if (folder?.Exists == true)
            {
                return folder;
            }
        }
        catch (FolderNotFoundException)
        {
            // Provider does not expose this special-use folder.
        }

        var folders = await transport.DiscoverFoldersAsync(client, ct);
        var attribute = specialFolder switch
        {
            SpecialFolder.Trash => FolderAttributes.Trash,
            SpecialFolder.Sent => FolderAttributes.Sent,
            SpecialFolder.Drafts => FolderAttributes.Drafts,
            SpecialFolder.Junk => FolderAttributes.Junk,
            SpecialFolder.Archive => FolderAttributes.Archive,
            _ => FolderAttributes.None
        };

        return attribute == FolderAttributes.None
            ? null
            : folders.FirstOrDefault(folder => folder.Attributes.HasFlag(attribute));
    }

    private static SearchQuery BuildSearchQuery(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return SearchQuery.All;
        }

        var term = search.Trim();
        return SearchQuery.SubjectContains(term)
            .Or(SearchQuery.FromContains(term))
            .Or(SearchQuery.ToContains(term))
            .Or(SearchQuery.BodyContains(term));
    }

    private MailFolderDto ToFolderDto(IMailFolder folder)
    {
        var isSpecial = folder.Attributes.HasFlag(FolderAttributes.Inbox)
            || folder.Attributes.HasFlag(FolderAttributes.Sent)
            || folder.Attributes.HasFlag(FolderAttributes.Drafts)
            || folder.Attributes.HasFlag(FolderAttributes.Trash)
            || folder.Attributes.HasFlag(FolderAttributes.Junk)
            || folder.Attributes.HasFlag(FolderAttributes.Archive);

        return new MailFolderDto
        {
            Key = transport.EncodeFolderKey(folder.FullName),
            Name = folder.Name,
            DisplayName = DisplayName(folder),
            UnreadCount = folder.Unread,
            IsSpecial = isSpecial,
            Icon = FolderIcon(folder)
        };
    }

    private static int FolderOrder(MailFolderDto folder)
    {
        if (folder.DisplayName == "Gelen Kutusu") return 0;
        if (folder.DisplayName == "Gönderilenler") return 1;
        if (folder.DisplayName == "Taslaklar") return 2;
        if (folder.DisplayName == "Arşiv") return 3;
        if (folder.DisplayName == "Spam") return 4;
        if (folder.DisplayName == "Çöp Kutusu") return 5;
        return 10;
    }

    private static string DisplayName(IMailFolder folder)
    {
        if (folder.Attributes.HasFlag(FolderAttributes.Inbox)) return "Gelen Kutusu";
        if (folder.Attributes.HasFlag(FolderAttributes.Sent)) return "Gönderilenler";
        if (folder.Attributes.HasFlag(FolderAttributes.Drafts)) return "Taslaklar";
        if (folder.Attributes.HasFlag(FolderAttributes.Archive)) return "Arşiv";
        if (folder.Attributes.HasFlag(FolderAttributes.Junk)) return "Spam";
        if (folder.Attributes.HasFlag(FolderAttributes.Trash)) return "Çöp Kutusu";
        return folder.Name;
    }

    private static string FolderIcon(IMailFolder folder)
    {
        if (folder.Attributes.HasFlag(FolderAttributes.Inbox)) return "fa-inbox";
        if (folder.Attributes.HasFlag(FolderAttributes.Sent)) return "fa-paper-plane";
        if (folder.Attributes.HasFlag(FolderAttributes.Drafts)) return "fa-file-pen";
        if (folder.Attributes.HasFlag(FolderAttributes.Archive)) return "fa-box-archive";
        if (folder.Attributes.HasFlag(FolderAttributes.Junk)) return "fa-ban";
        if (folder.Attributes.HasFlag(FolderAttributes.Trash)) return "fa-trash";
        return "fa-folder";
    }

    private MailMessageSummaryDto ToSummary(string folderKey, IMessageSummary summary)
    {
        var sender = summary.Envelope?.From?.Mailboxes.FirstOrDefault();
        var date = summary.Envelope?.Date ?? summary.InternalDate ?? DateTimeOffset.UtcNow;

        return new MailMessageSummaryDto
        {
            FolderKey = folderKey,
            Uid = summary.UniqueId.Id,
            SenderName = sender?.Name ?? string.Empty,
            SenderAddress = sender?.Address ?? string.Empty,
            Subject = string.IsNullOrWhiteSpace(summary.Envelope?.Subject) ? "(Konusuz)" : summary.Envelope.Subject,
            Preview = summary.PreviewText ?? string.Empty,
            Date = date,
            IsRead = summary.Flags?.HasFlag(MessageFlags.Seen) == true,
            HasAttachments = summary.Attachments?.Any() == true
        };
    }

    private MailMessageDetailDto ToDetail(
        string folderKey,
        uint uid,
        MimeMessage message,
        bool isRead)
    {
        var html = message.HtmlBody;
        var bodyHtml = string.IsNullOrWhiteSpace(html)
            ? $"<pre class=\"mail-plain-body\">{HtmlEncoder.Default.Encode(message.TextBody ?? string.Empty)}</pre>"
            : sanitizer.Sanitize(html);

        if (string.IsNullOrWhiteSpace(bodyHtml))
        {
            bodyHtml = "<p class=\"mail-empty-body\">Bu mesajın görüntülenebilir gövdesi yok.</p>";
        }

        return new MailMessageDetailDto
        {
            FolderKey = folderKey,
            Uid = uid,
            Subject = string.IsNullOrWhiteSpace(message.Subject) ? "(Konusuz)" : message.Subject,
            Date = message.Date,
            From = ToAddress(message.From.Mailboxes.FirstOrDefault()),
            To = message.To.Mailboxes.Select(ToAddress).ToArray(),
            Cc = message.Cc.Mailboxes.Select(ToAddress).ToArray(),
            ReplyTo = message.ReplyTo.Mailboxes.FirstOrDefault()?.Address,
            MessageId = message.MessageId,
            References = message.References is null ? null : string.Join(" ", message.References),
            BodyHtml = bodyHtml,
            BodyText = message.TextBody ?? string.Empty,
            Attachments = EnumerateMimeParts(message.Body)
                .Where(item => IsAttachment(item.Part))
                .Select(item => ToAttachment(item.Id, item.Part))
                .ToArray(),
            IsRead = isRead
        };
    }

    private static MailAddressDto ToAddress(MailboxAddress? address) => new()
    {
        Name = address?.Name ?? string.Empty,
        Address = address?.Address ?? string.Empty
    };

    private static MailAttachmentDto ToAttachment(string partId, MimePart part) => new()
    {
        PartId = partId,
        FileName = MailTransport.SanitizeFileName(part.FileName ?? "ek"),
        ContentType = part.ContentType.MimeType,
        Size = part.ContentDisposition?.Size ?? 0,
        IsInline = string.Equals(part.ContentDisposition?.Disposition, ContentDisposition.Inline, StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(part.ContentId)
    };

    private static bool IsAttachment(MimePart part) =>
        !string.IsNullOrWhiteSpace(part.FileName)
        || string.Equals(part.ContentDisposition?.Disposition, ContentDisposition.Attachment, StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(part.ContentId);

    private static IEnumerable<(string Id, MimePart Part)> EnumerateMimeParts(
        MimeEntity? entity,
        string id = "1")
    {
        if (entity is Multipart multipart)
        {
            for (var index = 0; index < multipart.Count; index++)
            {
                foreach (var part in EnumerateMimeParts(multipart[index], $"{id}.{index + 1}"))
                {
                    yield return part;
                }
            }

            yield break;
        }

        if (entity is MimePart mimePart)
        {
            yield return (id, mimePart);
        }
    }
}
