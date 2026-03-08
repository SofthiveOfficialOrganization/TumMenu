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
        var stores = await mediator.Send(new GetMyQRInfoQuery());
        ViewBag.Stores = stores;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetQRStats(QRStatsGranularity granularity, Guid? storeId, CancellationToken ct)
    {
        var stats = await mediator.Send(new GetQRScanStatsQuery(granularity, storeId), ct);
        return Json(stats);
    }

    [HttpGet]
    public async Task<IActionResult> GetProductStats(QRStatsGranularity granularity, Guid? storeId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductAnalysisQuery(granularity, storeId), ct);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoryStats(QRStatsGranularity granularity, Guid? storeId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryAnalysisQuery(granularity, storeId), ct);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetStoreBreakdown()
    {
        var result = await mediator.Send(new GetQRScanStatsPerStoreQuery());
        return Json(result);
    }
}
