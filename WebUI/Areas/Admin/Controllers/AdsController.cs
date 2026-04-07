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
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var slots = await mediator.Send(new GetAdSlotsQuery());
        return View(slots);
    }

    [HttpGet]
    public async Task<IActionResult> Creatives()
    {
        var creatives = await mediator.Send(new GetAdCreativesQuery());
        return View(creatives);
    }

    [HttpGet]
    public async Task<IActionResult> Placements()
    {
        var placements = await mediator.Send(new GetAdPlacementsQuery());
        return View(placements);
    }

    [HttpGet]
    public async Task<IActionResult> Revenue()
    {
        var summary = await mediator.Send(new GetAdRevenueSummaryQuery());
        return View(summary);
    }

    [HttpGet]
    public async Task<IActionResult> Analytics(int days = 30)
    {
        if (days != 7 && days != 30 && days != 90) days = 30;
        var analytics = await mediator.Send(new GetAdAnalyticsQuery(days));
        ViewBag.Days = days;
        return View(analytics);
    }

    [HttpPost]
    public async Task<IActionResult> ImportRevenue(IFormFile file)
    {
        if (file == null || !file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Lütfen geçerli bir .csv dosyası seçin.";
            return RedirectToAction(nameof(Revenue));
        }
        if (file.Length > 2 * 1024 * 1024)
        {
            TempData["Error"] = "Dosya boyutu 2MB'ı geçemez.";
            return RedirectToAction(nameof(Revenue));
        }
        var result = await mediator.Send(new ImportAdRevenueCommand { File = file });
        TempData["Success"] = $"{result.ImportedCount} satır içe aktarıldı, {result.SkippedCount} satır atlandı.";
        return RedirectToAction(nameof(Revenue));
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
        return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
    }

    [HttpGet]
    public async Task<IActionResult> EditSlot(Guid id)
    {
        var slots = await mediator.Send(new GetAdSlotsQuery());
        var slot = slots.Find(x => x.Id == id) ?? throw new Exception("Slot bulunamadı");
        return View(new UpdateAdSlotCommand { Id = slot.Id, Key = slot.Key, Description = slot.Description, IsActive = slot.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> EditSlot(UpdateAdSlotCommand command)
    {
        if (!ModelState.IsValid) return View(command);
        await mediator.Send(command);
        TempData["Success"] = "Reklam slotu güncellendi.";
        return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSlot(Guid id)
    {
        await mediator.Send(new DeleteAdSlotCommand { Id = id });
        TempData["Success"] = "Reklam slotu silindi.";
        return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
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
        return RedirectToAction(nameof(Creatives), new { role = RouteData.Values["role"] });
    }

    [HttpGet]
    public async Task<IActionResult> EditCreative(Guid id)
    {
        var creatives = await mediator.Send(new GetAdCreativesQuery());
        var creative = creatives.Find(x => x.Id == id) ?? throw new Exception("Kreatif bulunamadı");
        return View(new UpdateAdCreativeCommand { Id = creative.Id, Type = creative.Type, Content = creative.Content, ClickUrl = creative.ClickUrl });
    }

    [HttpPost]
    public async Task<IActionResult> EditCreative(UpdateAdCreativeCommand command)
    {
        if (!ModelState.IsValid) return View(command);
        await mediator.Send(command);
        TempData["Success"] = "Reklam içeriği güncellendi.";
        return RedirectToAction(nameof(Creatives), new { role = RouteData.Values["role"] });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCreative(Guid id)
    {
        await mediator.Send(new DeleteAdCreativeCommand { Id = id });
        TempData["Success"] = "Reklam içeriği silindi.";
        return RedirectToAction(nameof(Creatives), new { role = RouteData.Values["role"] });
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
        return RedirectToAction(nameof(Placements), new { role = RouteData.Values["role"] });
    }

    [HttpGet]
    public async Task<IActionResult> EditPlacement(Guid id)
    {
        var placements = await mediator.Send(new GetAdPlacementsQuery());
        var p = placements.Find(x => x.Id == id) ?? throw new Exception("Yerleştirme bulunamadı");
        ViewBag.Slots = await mediator.Send(new GetAdSlotsQuery());
        ViewBag.Creatives = await mediator.Send(new GetAdCreativesQuery());
        return View(new UpdateAdPlacementCommand { Id = p.Id, AdSlotId = p.AdSlotId, AdCreativeId = p.AdCreativeId, StartAt = p.StartAt, EndAt = p.EndAt, DailyCap = p.DailyCap, IsActive = p.IsActive });
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
        return RedirectToAction(nameof(Placements), new { role = RouteData.Values["role"] });
    }

    [HttpPost]
    public async Task<IActionResult> DeletePlacement(Guid id)
    {
        await mediator.Send(new DeleteAdPlacementCommand { Id = id });
        TempData["Success"] = "Reklam yerleşimi silindi.";
        return RedirectToAction(nameof(Placements), new { role = RouteData.Values["role"] });
    }
    #endregion
}


