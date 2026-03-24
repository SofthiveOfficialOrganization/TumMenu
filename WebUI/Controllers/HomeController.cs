using Application.Categories.Queries;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebUI.Models;

namespace WebUI.Controllers;

public class HomeController(IMediator mediator, IEmailSender emailSender) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var homepageStores = await mediator.Send(new GetHomepageStoresQuery(), ct);
        ViewBag.HomepageStores = homepageStores;

        // Load category library items for map filter
        var categories = await mediator.Send(
            new GetAllCategoryLibraryItemsPagedQuery { Page = 1, PageSize = 100 }, ct);
        ViewBag.Categories = categories.Items.ToList();

        // Add platform cookie to identify legitimate platform users
        HttpContext.Response.Cookies.Append("FromTumMenuPlatform", "true", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddHours(2), // Short lived cookie
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return View();
    }

    public IActionResult Privacy()
    {
        return Redirect("/Home/Legal#section-privacy");
    }
    public IActionResult BeQr()
    {
        return View();
    }
    public IActionResult WhatToEat()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var htmlMessage = $@"
                <h3>İletişim Formundan Yeni Mesaj</h3>
                <p><strong>Ad Soyad:</strong> {model.Name}</p>
                <p><strong>E-posta:</strong> {model.Email}</p>
                <p><strong>Konu:</strong> {model.Subject}</p>
                <hr />
                <p><strong>Mesaj:</strong></p>
                <p>{model.Message}</p>
            ";

            // Destek mailine gönder
            await emailSender.SendEmailAsync("destek@tummenu.com.tr", $"İletişim Formu: {model.Subject}", htmlMessage);

            TempData["Success"] = "Mesajınız başarıyla gönderildi! En kısa sürede size dönüş yapacağız.";
            return RedirectToAction(nameof(Contact));
        }
        catch
        {
            // Log error
            TempData["Error"] = "Mesaj gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
            return View(model);
        }
    }

    public new IActionResult NotFound()
    {
        return View();
    }

    public async Task<IActionResult> Restaurants(CancellationToken ct)
    {
        // Load category library items for filter chips
        var categories = await mediator.Send(
            new GetAllCategoryLibraryItemsPagedQuery { Page = 1, PageSize = 100 }, ct);
        ViewBag.Categories = categories.Items.ToList();
        // Add platform cookie to identify legitimate platform users
        HttpContext.Response.Cookies.Append("FromTumMenuPlatform", "true", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddHours(2),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return View();
    }

    [HttpGet("api/stores/search")]
    public async Task<IActionResult> SearchStores([FromQuery] SearchStoresQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Json(result);
    }

    [HttpGet("api/categories/search")]
    public async Task<IActionResult> SearchCategories(string? search, int page = 1, int pageSize = 15, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery
        {
            Search = search,
            Page = page,
            PageSize = pageSize
        }, ct);

        var items = result.Items.Select(i => new { id = i.Id, text = i.Title });
        var hasMore = result.HasNext;

        return Json(new { results = items, pagination = new { more = hasMore } });
    }

    [HttpGet]
    public IActionResult Legal()
    {
        return View();
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Pricing()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
