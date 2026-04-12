using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.Companies.Queries;
using Application.Support.Commands;
using Application.Support.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOnly")]
public class SupportController(IMediator mediator, IEmailSender emailSender) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportIssue([FromForm] OwnerIssueReportViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?? "Form bilgileri doğrulanamadı.";

            return BadRequest(new { message = firstError });
        }

        try
        {
            // DB'ye kaydet
            var reportId = await mediator.Send(new CreateOwnerIssueReportCommand(
                model.Description,
                model.AttemptedAction,
                model.CurrentPageTitle,
                model.CurrentPageUrl,
                model.BrowserInfo,
                model.Viewport
            ), ct);

            // E-posta gönder
            var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
            var ownerName = User.Identity?.Name ?? "Bilinmeyen kullanıcı";
            var ownerEmail = User.FindFirstValue(ClaimTypes.Email) ?? "-";
            var ownerId = User.FindFirstValue("owner_id") ?? "-";

            var subject = $"Owner Hata Bildirimi - {company?.Title ?? ownerName}";
            var htmlMessage = $"""
                <h3>Owner Paneli Hata Bildirimi</h3>
                <p><strong>Kayıt ID:</strong> {reportId}</p>
                <p><strong>Kullanıcı:</strong> {Encode(ownerName)}</p>
                <p><strong>E-posta:</strong> {Encode(ownerEmail)}</p>
                <p><strong>Owner Id:</strong> {Encode(ownerId)}</p>
                <p><strong>Şirket:</strong> {Encode(company?.Title ?? "-")}</p>
                <p><strong>Sayfa:</strong> {Encode(model.CurrentPageTitle)}</p>
                <p><strong>Bağlantı:</strong> <a href="{Encode(model.CurrentPageUrl)}">{Encode(model.CurrentPageUrl)}</a></p>
                <p><strong>Ekran Boyutu:</strong> {Encode(model.Viewport ?? "-")}</p>
                <p><strong>Tarayıcı:</strong> {Encode(model.BrowserInfo ?? "-")}</p>
                <p><strong>Bildirim Zamanı:</strong> {DateTimeOffset.Now:dd.MM.yyyy HH:mm:ss}</p>
                <hr />
                <p><strong>Karşılaşılan Hata:</strong></p>
                <p>{FormatMultiline(model.Description)}</p>
                <p><strong>Yapmak İstediği İşlem:</strong></p>
                <p>{FormatMultiline(model.AttemptedAction)}</p>
                """;

            await emailSender.SendEmailAsync("softhiveyonetim@gmail.com", subject, htmlMessage);

            return Ok(new { message = "Hata bildiriminiz destek ekibine iletildi." });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Bildirim gönderilirken bir hata oluştu. Lütfen tekrar deneyin."
            });
        }
    }

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);

    private static string FormatMultiline(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        return HtmlEncoder.Default.Encode(value)
            .Replace("\r\n", "<br />", StringComparison.Ordinal)
            .Replace("\n", "<br />", StringComparison.Ordinal);
    }
}
