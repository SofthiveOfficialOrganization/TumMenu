using Application.SystemSettings.Commands;
using Application.SystemSettings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SystemSettingsController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var settings = await mediator.Send(new GetLegalVersionSettingsQuery(), ct);
        return View(new UpdateLegalVersionSettingsCommand
        {
            TermsVersion = settings.TermsVersion,
            TermsDescription = settings.TermsDescription,
            KvkkVersion = settings.KvkkVersion,
            KvkkDescription = settings.KvkkDescription,
            PrivacyVersion = settings.PrivacyVersion,
            PrivacyDescription = settings.PrivacyDescription,
            CookieVersion = settings.CookieVersion,
            CookieDescription = settings.CookieDescription
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateLegalVersionSettingsCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(command);
        }

        await mediator.Send(command, ct);
        TempData["Success"] = "Sistem ayarları güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
