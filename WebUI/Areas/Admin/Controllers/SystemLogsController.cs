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
