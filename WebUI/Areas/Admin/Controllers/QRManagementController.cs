using Application.QRs.Queries;
using Application.QRs.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public class QRManagementController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (User.IsInRole("Admin"))
        {
            var qrs = await mediator.Send(new GetQRCodesQuery(), ct);
            return View("Index", qrs);
        }
        else
        {
            var model = await mediator.Send(new GetMyQRInfoQuery(), ct);
            if (model == null || !model.Any())
            {
                return View("NoQR");
            }
            return View("MyQR", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> StoreDetails(Guid storeId, CancellationToken ct)
    {
        var model = await mediator.Send(new GetQRDetailQuery(storeId, User.IsInRole("Admin")), ct);
        if (model == null)
        {
            return NotFound();
        }
        return View("Details", model);
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
            return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
        }

        var count = await mediator.Send(new GlobalUpdateQRBaseDomainCommand { NewBaseDomain = newBaseDomain }, ct);
        TempData["Success"] = $"{count} adet QR kodunun base domain'i başarıyla güncellendi.";
        return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
    }
}




