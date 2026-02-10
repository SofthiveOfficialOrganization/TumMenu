using Application.Categories.Queries;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebUI.Models;

namespace WebUI.Controllers;

public class HomeController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var homepageStores = await mediator.Send(new GetHomepageStoresQuery(), ct);
        ViewBag.HomepageStores = homepageStores;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult BeQr()
    {
        return View();
    }
    public IActionResult WhatToEat()
    {
        return View();
    }

    public IActionResult NotFound()
    {
        return View();
    }

    public async Task<IActionResult> Restaurants(CancellationToken ct)
    {
        // Load category library items for filter chips
        var categories = await mediator.Send(
            new GetAllCategoryLibraryItemsPagedQuery { Page = 0, PageSize = 100 }, ct);
        ViewBag.Categories = categories.Items.ToList();
        return View();
    }

    [HttpGet("api/stores/search")]
    public async Task<IActionResult> SearchStores([FromQuery] SearchStoresQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Json(result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
