using Application.QRs.Queries;
using Application.Products.Queries;
using Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class AnalysisController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        if (isAdmin)
        {
            var companies = await mediator.Send(new GetAdminCompaniesQuery());
            ViewBag.Companies = companies;
            ViewBag.IsAdmin = true;
        }
        else
        {
            var stores = await mediator.Send(new GetMyQRInfoQuery());
            ViewBag.Stores = stores;
            ViewBag.IsAdmin = false;
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetQRStats(QRStatsGranularity granularity, Guid? storeId, Guid? companyId, CancellationToken ct)
    {
        var stats = await mediator.Send(new GetQRScanStatsQuery(granularity, storeId, companyId), ct);
        return Json(stats);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductStats(QRStatsGranularity granularity, Guid? storeId, Guid? companyId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductAnalysisQuery(granularity, storeId, companyId), ct);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoryStats(QRStatsGranularity granularity, Guid? storeId, Guid? companyId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryAnalysisQuery(granularity, storeId, companyId), ct);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetStoreBreakdown(Guid? companyId)
    {
        var result = await mediator.Send(new GetQRScanStatsPerStoreQuery(companyId));
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetStoresByCompany(Guid? companyId)
    {
        if (!companyId.HasValue) return Json(new List<object>());
        var result = await mediator.Send(new GetStoresByCompanyQuery(companyId.Value));
        return Json(result);
    }
}
