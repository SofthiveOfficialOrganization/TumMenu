using Application.CustomerOrderRequests.Commands;
using Application.CustomerOrderRequests.Queries;
using Application.Stores.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public sealed class OrderRequestsController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CustomerOrderRequestStatus? status, Guid? storeId, DateTime? date, CancellationToken ct)
    {
        ViewData["Title"] = "Gelen Siparişler";
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentStoreId = storeId;
        ViewBag.CurrentDate = date?.ToString("yyyy-MM-dd");
        ViewBag.StoreOptions = await mediator.Send(new GetStoresPagedQuery
        {
            Page = 1,
            PageSize = 200
        }, ct);

        var orders = await mediator.Send(new GetAdminOrderRequestsQuery
        {
            Status = status,
            StoreId = storeId,
            Date = date
        }, ct);

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var order = await mediator.Send(new GetOrderRequestDetailQuery(id), ct);
        return Json(order);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MarkSeen(Guid id, CancellationToken ct)
    {
        await mediator.Send(new MarkOrderRequestSeenCommand(id), ct);
        return Json(new { success = true, status = CustomerOrderRequestStatus.Seen.ToString() });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CompleteOrderRequestCommand(id), ct);
        return Json(new { success = true, status = CustomerOrderRequestStatus.Completed.ToString() });
    }
}
