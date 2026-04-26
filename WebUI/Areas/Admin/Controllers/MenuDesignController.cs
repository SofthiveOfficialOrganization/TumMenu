using Application.MenuDesigns.Commands;
using Application.MenuDesigns.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class MenuDesignController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var designs = await mediator.Send(new GetAllMenuDesignsQuery(), ct);
        return View(designs);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateMenuDesignCommand());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateMenuDesignCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(cmd);
        await mediator.Send(cmd, ct);
        TempData["Success"] = "Tasarım başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetMenuDesignByIdQuery(id), ct);
        var cmd = new UpdateMenuDesignCommand
        {
            Id = dto.Id,
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            PrimaryColor = dto.PrimaryColor,
            PrimaryDarkColor = dto.PrimaryDarkColor,
            AccentColor = dto.AccentColor,
            BackgroundColor = dto.BackgroundColor,
            SurfaceColor = dto.SurfaceColor,
            TextColor = dto.TextColor,
            MutedColor = dto.MutedColor,
            BorderRadius = dto.BorderRadius,
            BackgroundGradient = dto.BackgroundGradient,
            PreviewImageUrl = dto.PreviewImageUrl,
            IsDefault = dto.IsDefault,
            SortOrder = dto.SortOrder
        };
        return View(cmd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] UpdateMenuDesignCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(cmd);
        await mediator.Send(cmd, ct);
        TempData["Success"] = "Tasarım güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMenuDesignCommand { Id = id }, ct);
        TempData["Success"] = "Tasarım silindi.";
        return RedirectToAction(nameof(Index));
    }
}
