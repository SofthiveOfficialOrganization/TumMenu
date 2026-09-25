using WebUI.Models.Mail;

namespace WebUI.ExternalServices;

public interface IMailboxService
{
    Task<IReadOnlyList<MailFolderDto>> GetFoldersAsync(CancellationToken ct = default);

    Task<MailMessageListResult> ListMessagesAsync(
        string folderKey,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<MailMessageDetailDto> GetMessageAsync(
        string folderKey,
        uint uid,
        bool markAsRead = true,
        CancellationToken ct = default);

    Task MarkReadAsync(string folderKey, uint uid, bool isRead, CancellationToken ct = default);

    Task BulkMarkReadAsync(
        string folderKey,
        IReadOnlyCollection<uint> uids,
        bool isRead,
        CancellationToken ct = default);

    Task MoveAsync(string sourceFolderKey, uint uid, string destinationFolderKey, CancellationToken ct = default);

    Task BulkMoveAsync(
        string sourceFolderKey,
        IReadOnlyCollection<uint> uids,
        string destinationFolderKey,
        CancellationToken ct = default);

    Task DeleteAsync(string folderKey, uint uid, CancellationToken ct = default);

    Task BulkDeleteAsync(string folderKey, IReadOnlyCollection<uint> uids, CancellationToken ct = default);

    Task<MailAttachmentContent> DownloadAttachmentAsync(
        string folderKey,
        uint uid,
        string partId,
        CancellationToken ct = default);

    Task<MailSendResult> SendAsync(MailComposeRequest request, CancellationToken ct = default);
}
