using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebUI.Models.Mail;

public sealed class MailFolderDto
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int UnreadCount { get; init; }
    public bool IsSpecial { get; init; }
    public string Icon { get; init; } = "fa-folder";
}

public sealed class MailMessageSummaryDto
{
    public string FolderKey { get; init; } = string.Empty;
    public uint Uid { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string SenderAddress { get; init; } = string.Empty;
    public string Subject { get; init; } = "(Konusuz)";
    public string Preview { get; init; } = string.Empty;
    public DateTimeOffset Date { get; init; }
    public bool IsRead { get; init; }
    public bool HasAttachments { get; init; }
}

public sealed class MailAddressDto
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;

    public string Display => string.IsNullOrWhiteSpace(Name) ? Address : $"{Name} <{Address}>";
}

public sealed class MailAttachmentDto
{
    public string PartId { get; init; } = string.Empty;
    public string FileName { get; init; } = "ek";
    public string ContentType { get; init; } = "application/octet-stream";
    public long Size { get; init; }
    public bool IsInline { get; init; }
}

public sealed class MailMessageDetailDto
{
    public string FolderKey { get; init; } = string.Empty;
    public uint Uid { get; init; }
    public string Subject { get; init; } = "(Konusuz)";
    public DateTimeOffset Date { get; init; }
    public MailAddressDto From { get; init; } = new();
    public IReadOnlyList<MailAddressDto> To { get; init; } = [];
    public IReadOnlyList<MailAddressDto> Cc { get; init; } = [];
    public string? ReplyTo { get; init; }
    public string? MessageId { get; init; }
    public string? References { get; init; }
    public string BodyHtml { get; init; } = string.Empty;
    public string BodyText { get; init; } = string.Empty;
    public IReadOnlyList<MailAttachmentDto> Attachments { get; init; } = [];
    public bool IsRead { get; init; }
}

public sealed class MailMessageListResult
{
    public IReadOnlyList<MailMessageSummaryDto> Messages { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize)));
}

public sealed class MailAttachmentContent
{
    public string FileName { get; init; } = "ek";
    public string ContentType { get; init; } = "application/octet-stream";
    public Stream Content { get; init; } = Stream.Null;
}

public sealed class MailSendResult
{
    public bool Sent { get; init; }
    public bool Archived { get; init; }
    public string? Warning { get; init; }
}

public sealed record MailPageViewModel
{
    public IReadOnlyList<MailFolderDto> Folders { get; init; } = [];
    public IReadOnlyList<MailMessageSummaryDto> Messages { get; init; } = [];
    public MailMessageDetailDto? SelectedMessage { get; init; }
    public string CurrentFolderKey { get; init; } = string.Empty;
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
    public int TotalCount { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class MailComposeRequest
{
    [Required(ErrorMessage = "En az bir alıcı girin.")]
    [MaxLength(4000)]
    public string To { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Cc { get; set; }

    [MaxLength(4000)]
    public string? Bcc { get; set; }

    [Required(ErrorMessage = "Konu zorunludur.")]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mesaj gövdesi zorunludur.")]
    [MaxLength(500_000)]
    public string BodyHtml { get; set; } = string.Empty;

    public string? InReplyTo { get; set; }

    public string? References { get; set; }

    public List<IFormFile> Attachments { get; set; } = [];
}
