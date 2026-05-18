using Application.SystemLogs.Commands;
using Application.SystemLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class SystemLogsController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(GetSystemLogsQuery query, CancellationToken ct)
    {
        var logs = await mediator.Send(query, ct);
        return View(logs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(DateOnly? dateFrom, DateOnly? dateTo, int[] statusCodes, CancellationToken ct)
    {
        if (!dateFrom.HasValue || !dateTo.HasValue)
        {
            TempData["Error"] = "Log silmek için başlangıç ve bitiş tarihi seçin.";
            return RedirectToAction(nameof(Index));
        }

        if (dateTo.Value < dateFrom.Value)
        {
            TempData["Error"] = "Bitiş tarihi başlangıç tarihinden önce olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var deletedCount = await mediator.Send(new ClearSystemLogsCommand
        {
            DateFrom = dateFrom.Value,
            DateTo = dateTo.Value,
            StatusCodes = statusCodes
        }, ct);

        TempData["Success"] = deletedCount == 0
            ? "Seçili kriterlere uygun log bulunamadı."
            : $"{deletedCount} log kaydı silindi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var log = await mediator.Send(new GetSystemLogDetailQuery(id), ct);
        if (log is null)
        {
            return NotFound();
        }

        return View(log);
    }
}
