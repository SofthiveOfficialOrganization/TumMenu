using System.Text.Encodings.Web;
using Application.Support.Commands;
using Application.Support.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminSupportController(IMediator mediator, IEmailSender emailSender) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(IssueReportStatus? status, CancellationToken ct)
    {
        var reports = await mediator.Send(new GetOwnerIssueReportsQuery(status), ct);
        ViewBag.CurrentStatus = status;
        return View(reports);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        IssueReportStatus status,
        string? adminNote,
        IssueReportStatus? returnStatus,
        CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateIssueStatusCommand(id, status, adminNote), ct);

        if (result.JustResolved && !string.IsNullOrWhiteSpace(result.OwnerEmail))
        {
            Func<string, string> encode = HtmlEncoder.Default.Encode;
            var noteHtml = string.IsNullOrWhiteSpace(result.AdminNote)
                ? "<p>Destek ekibimiz sorununuzu inceledi ve çözüme kavuşturdu.</p>"
                : $"<p><strong>Destek Notu:</strong><br />{encode(result.AdminNote).Replace("\n", "<br />")}</p>";

            var htmlMessage = $"""
                <h3>Destek Talebiniz Çözüldü</h3>
                <p>Merhaba <strong>{encode(result.OwnerName)}</strong>,</p>
                <p>İlettiğiniz destek talebi başarıyla çözüme kavuşturuldu.</p>
                {noteHtml}
                <hr />
                <p><strong>Talep Konusu:</strong></p>
                <p>{encode(result.Description)}</p>
                <hr />
                <p>Başka bir sorununuz olursa bizimle iletişime geçmekten çekinmeyin.</p>
                <p>İyi günler,<br />TumMenu Destek Ekibi</p>
                """;

            await emailSender.SendEmailAsync(
                result.OwnerEmail,
                "Destek Talebiniz Çözüldü – TumMenu",
                htmlMessage
            );
        }

        TempData["Success"] = "Durum güncellendi.";
        return RedirectToAction(nameof(Index), new { status = returnStatus });
    }
}
