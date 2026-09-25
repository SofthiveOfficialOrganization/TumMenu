using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.ExternalServices;
using WebUI.Models.Mail;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class MailController(
    IMailboxService mailbox,
    ILogger<MailController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? folder,
        string? q,
        int page = 1,
        uint? uid = null,
        CancellationToken ct = default)
    {
        var model = new MailPageViewModel
        {
            Search = q,
            Page = Math.Max(1, page)
        };

        try
        {
            var folders = await mailbox.GetFoldersAsync(ct);
            var currentFolder = folders.FirstOrDefault(item => item.Key == folder)
                ?? folders.FirstOrDefault(item => item.DisplayName == "Gelen Kutusu")
                ?? folders.FirstOrDefault();

            if (currentFolder is null)
            {
                return View(model with { ErrorMessage = "Mailbox içinde erişilebilir bir klasör bulunamadı." });
            }

            var messageList = await mailbox.ListMessagesAsync(
                currentFolder.Key,
                q,
                model.Page,
                50,
                ct);

            MailMessageDetailDto? selected = null;
            if (uid.HasValue)
            {
                selected = await mailbox.GetMessageAsync(currentFolder.Key, uid.Value, markAsRead: true, ct);
            }

            model = new MailPageViewModel
            {
                Folders = folders,
                Messages = messageList.Messages,
                SelectedMessage = selected,
                CurrentFolderKey = currentFolder.Key,
                Search = q,
                Page = messageList.Page,
                TotalPages = messageList.TotalPages,
                TotalCount = messageList.TotalCount
            };
        }
        catch (Exception ex) when (ex is MailboxUnavailableException or MailValidationException)
        {
            logger.LogWarning(ex, "Admin mail ekranı yüklenemedi.");
            model = model with { ErrorMessage = ex.Message };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Admin mail ekranı yüklenirken beklenmeyen hata oluştu.");
            model = model with
            {
                ErrorMessage = "Mail kutusuna bağlanırken beklenmeyen bir hata oluştu. Lütfen tekrar deneyin."
            };
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Send(
        MailComposeRequest request,
        string? returnFolder,
        string? search,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message)));
            return RedirectToMail(returnFolder, search);
        }

        try
        {
            var result = await mailbox.SendAsync(request, ct);
            TempData[result.Archived ? "Success" : "Warning"] = result.Warning
                ?? "Mail başarıyla gönderildi ve Gönderilenler klasörüne kaydedildi.";
        }
        catch (Exception ex) when (ex is MailValidationException or MailboxUnavailableException)
        {
            logger.LogWarning(ex, "Admin mail gönderemedi.");
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Admin mail gönderimi başarısız oldu.");
            TempData["Error"] = "Mail gönderilemedi. SMTP ayarlarını ve bağlantıyı kontrol edin.";
        }

        return RedirectToMail(returnFolder, search);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(
        string folder,
        uint uid,
        bool isRead,
        string? search,
        CancellationToken ct)
    {
        try
        {
            await mailbox.MarkReadAsync(folder, uid, isRead, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Mail okunma durumu değiştirilemedi.");
            TempData["Error"] = "Mail durumu güncellenemedi.";
        }

        return RedirectToMail(folder, search);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(
        string folder,
        uint uid,
        string destinationFolder,
        string? search,
        CancellationToken ct)
    {
        try
        {
            await mailbox.MoveAsync(folder, uid, destinationFolder, ct);
            TempData["Success"] = "Mail taşındı.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Mail taşınamadı.");
            TempData["Error"] = "Mail taşınamadı.";
        }

        return RedirectToMail(folder, search);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        string folder,
        uint uid,
        string? search,
        CancellationToken ct)
    {
        try
        {
            await mailbox.DeleteAsync(folder, uid, ct);
            TempData["Success"] = "Mail çöp kutusuna taşındı.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Mail silinemedi.");
            TempData["Error"] = "Mail silinemedi.";
        }

        return RedirectToMail(folder, search);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestFormLimits(ValueCountLimit = 100)]
    public async Task<IActionResult> Bulk(
        string folder,
        uint[]? uids,
        string operation,
        string? destinationFolder,
        string? search,
        CancellationToken ct)
    {
        var selectedUids = uids?.Distinct().ToArray() ?? [];
        if (selectedUids.Length is < 1 or > 50 || selectedUids.Any(uid => uid == 0))
        {
            TempData["Error"] = "Bir işlemde 1 ile 50 arasında mail seçin.";
            return RedirectToMail(folder, search);
        }

        try
        {
            switch (operation)
            {
                case "mark-read":
                    await mailbox.BulkMarkReadAsync(folder, selectedUids, isRead: true, ct);
                    TempData["Success"] = $"{selectedUids.Length} mail okundu olarak işaretlendi.";
                    break;
                case "mark-unread":
                    await mailbox.BulkMarkReadAsync(folder, selectedUids, isRead: false, ct);
                    TempData["Success"] = $"{selectedUids.Length} mail okunmadı olarak işaretlendi.";
                    break;
                case "move" when !string.IsNullOrWhiteSpace(destinationFolder):
                    await mailbox.BulkMoveAsync(folder, selectedUids, destinationFolder, ct);
                    TempData["Success"] = $"{selectedUids.Length} mail taşındı.";
                    break;
                case "delete":
                    await mailbox.BulkDeleteAsync(folder, selectedUids, ct);
                    TempData["Success"] = $"{selectedUids.Length} mail çöp kutusuna taşındı.";
                    break;
                case "move":
                    TempData["Error"] = "Taşımak için hedef klasör seçin.";
                    break;
                default:
                    TempData["Error"] = "Geçersiz toplu işlem.";
                    break;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Toplu mail işlemi başarısız oldu: {Operation}", operation);
            TempData["Error"] = "Seçilen maillere işlem uygulanamadı. Lütfen tekrar deneyin.";
        }

        return RedirectToMail(folder, search);
    }

    [HttpGet]
    public async Task<IActionResult> Attachment(
        string folder,
        uint uid,
        string partId,
        CancellationToken ct)
    {
        try
        {
            var attachment = await mailbox.DownloadAttachmentAsync(folder, uid, partId, ct);
            return File(attachment.Content, attachment.ContentType, attachment.FileName, enableRangeProcessing: true);
        }
        catch (Exception ex) when (ex is MailValidationException or MailboxUnavailableException)
        {
            logger.LogWarning(ex, "Mail eki indirilemedi.");
            return BadRequest(ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Mail eki indirilirken hata oluştu.");
            return StatusCode(StatusCodes.Status502BadGateway, "Ek dosyası alınamadı.");
        }
    }

    private RedirectToActionResult RedirectToMail(string? folder, string? search)
    {
        return RedirectToAction(nameof(Index), new { folder, q = search });
    }
}
