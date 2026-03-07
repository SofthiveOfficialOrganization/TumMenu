using Application.QRs.Queries;
using Application.QRs.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/qr-yonetimi")]
public class QRManagementController(IMediator mediator) : Controller
{
    [HttpGet]
    [Authorize(Roles = "Owner")]
    [Route("qr-kodlarim")]
    public async Task<IActionResult> MyQR(CancellationToken ct)
    {
        var model = await mediator.Send(new GetMyQRInfoQuery(), ct);
        if (model == null || !model.Any())
        {
            return View("NoQR");
        }
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    [Route("detay/{storeId}")]
    public async Task<IActionResult> StoreDetails(Guid storeId, CancellationToken ct)
    {
        var model = await mediator.Send(new GetQRDetailQuery(storeId), ct);
        if (model == null)
        {
            return NotFound();
        }
        return View("Details", model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var qrs = await mediator.Send(new GetQRCodesQuery(), ct);
        return View(qrs);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route("global-domain-guncelle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GlobalUpdateBaseDomain(string newBaseDomain, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newBaseDomain))
        {
            TempData["Error"] = "Base domain boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var count = await mediator.Send(new GlobalUpdateQRBaseDomainCommand(newBaseDomain), ct);
        TempData["Success"] = $"{count} adet QR kodunun base domain'i başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
