using Application.Ads.Commands;
using Application.Ads.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var slots = await mediator.Send(new GetAdSlotsQuery());
        return View(slots);
    }

    public async Task<IActionResult> Creatives()
    {
        var creatives = await mediator.Send(new GetAdCreativesQuery());
        return View(creatives);
    }

    public async Task<IActionResult> Placements()
    {
        var placements = await mediator.Send(new GetAdPlacementsQuery());
        return View(placements);
    }

    public async Task<IActionResult> Revenue()
    {
        var summary = await mediator.Send(new GetAdRevenueSummaryQuery());
        return View(summary);
    }

    #region AdSlot Actions
    [HttpGet]
    public IActionResult CreateSlot() => View();

    [HttpPost]
    public async Task<IActionResult> CreateSlot(CreateAdSlotCommand command)
    {
        if (!ModelState.IsValid) return View(command);
        await mediator.Send(command);
        TempData["Success"] = "Reklam slotu başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditSlot(Guid id)
    {
        var slots = await mediator.Send(new GetAdSlotsQuery());
        var slot = slots.Find(x => x.Id == id) ?? throw new Exception("Slot not found");
        return View(new UpdateAdSlotCommand(slot.Id, slot.Key, slot.Description, slot.IsActive));
    }

    [HttpPost]
    public async Task<IActionResult> EditSlot(UpdateAdSlotCommand command)
    {
        if (!ModelState.IsValid) return View(command);
        await mediator.Send(command);
        TempData["Success"] = "Reklam slotu güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSlot(Guid id)
    {
        await mediator.Send(new DeleteAdSlotCommand(id));
        TempData["Success"] = "Reklam slotu silindi.";
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region AdCreative Actions
    [HttpGet]
    public IActionResult CreateCreative() => View();

    [HttpPost]
    public async Task<IActionResult> CreateCreative(CreateAdCreativeCommand command)
    {
        if (!ModelState.IsValid) return View(command);
        await mediator.Send(command);
        TempData["Success"] = "Reklam içeriği başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Creatives));
    }

    [HttpGet]
    public async Task<IActionResult> EditCreative(Guid id)
    {
        var creatives = await mediator.Send(new GetAdCreativesQuery());
        var creative = creatives.Find(x => x.Id == id) ?? throw new Exception("Creative not found");
        return View(new UpdateAdCreativeCommand(creative.Id, creative.Type, creative.Content, creative.ClickUrl));
    }

    [HttpPost]
    public async Task<IActionResult> EditCreative(UpdateAdCreativeCommand command)
    {
        if (!ModelState.IsValid) return View(command);
        await mediator.Send(command);
        TempData["Success"] = "Reklam içeriği güncellendi.";
        return RedirectToAction(nameof(Creatives));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCreative(Guid id)
    {
        await mediator.Send(new DeleteAdCreativeCommand(id));
        TempData["Success"] = "Reklam içeriği silindi.";
        return RedirectToAction(nameof(Creatives));
    }
    #endregion

    #region AdPlacement Actions
    [HttpGet]
    public async Task<IActionResult> CreatePlacement()
    {
        ViewBag.Slots = await mediator.Send(new GetAdSlotsQuery());
        ViewBag.Creatives = await mediator.Send(new GetAdCreativesQuery());
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlacement(CreateAdPlacementCommand command)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Slots = await mediator.Send(new GetAdSlotsQuery());
            ViewBag.Creatives = await mediator.Send(new GetAdCreativesQuery());
            return View(command);
        }
        await mediator.Send(command);
        TempData["Success"] = "Reklam yerleşimi başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Placements));
    }

    [HttpGet]
    public async Task<IActionResult> EditPlacement(Guid id)
    {
        var placements = await mediator.Send(new GetAdPlacementsQuery());
        var p = placements.Find(x => x.Id == id) ?? throw new Exception("Placement not found");
        ViewBag.Slots = await mediator.Send(new GetAdSlotsQuery());
        ViewBag.Creatives = await mediator.Send(new GetAdCreativesQuery());
        return View(new UpdateAdPlacementCommand(p.Id, p.AdSlotId, p.AdCreativeId, p.StartAt, p.EndAt, p.DailyCap, p.IsActive));
    }

    [HttpPost]
    public async Task<IActionResult> EditPlacement(UpdateAdPlacementCommand command)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Slots = await mediator.Send(new GetAdSlotsQuery());
            ViewBag.Creatives = await mediator.Send(new GetAdCreativesQuery());
            return View(command);
        }
        await mediator.Send(command);
        TempData["Success"] = "Reklam yerleşimi güncellendi.";
        return RedirectToAction(nameof(Placements));
    }

    [HttpPost]
    public async Task<IActionResult> DeletePlacement(Guid id)
    {
        await mediator.Send(new DeleteAdPlacementCommand(id));
        TempData["Success"] = "Reklam yerleşimi silindi.";
        return RedirectToAction(nameof(Placements));
    }
    #endregion
}
