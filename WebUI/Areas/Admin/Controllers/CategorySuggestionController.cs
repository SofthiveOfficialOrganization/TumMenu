using Application.CategorySuggestions.Commands;
using Application.CategorySuggestions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/category-suggestions")]
public class CategorySuggestionController(IMediator mediator) : Controller
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Index(int? status, int page = 0, int pageSize = 12, CancellationToken ct = default)
    {
        // Tüm Pending önerileri Viewed yap
        await mediator.Send(new MarkSuggestionsViewedCommand(), ct);

        var suggestions = await mediator.Send(new GetCategorySuggestionsQuery
        {
            StatusFilter = status,
            Page = page,
            PageSize = pageSize
        }, ct);

        ViewBag.StatusFilter = status;
        ViewBag.PageSize = pageSize;
        return View(suggestions);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("approve/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ApproveCategorySuggestionCommand { Id = id }, ct);
        TempData["Success"] = "Öneri onaylandı.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reject/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string? adminNote, CancellationToken ct)
    {
        await mediator.Send(new RejectCategorySuggestionCommand { Id = id, AdminNote = adminNote }, ct);
        TempData["Success"] = "Öneri reddedildi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("submit")]
    public IActionResult Submit()
    {
        return View();
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitCategorySuggestionCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(cmd);

        await mediator.Send(cmd, ct);
        TempData["Success"] = "Öneriniz alındı. Teşekkürler!";
        return RedirectToAction(nameof(Submit));
    }
}
