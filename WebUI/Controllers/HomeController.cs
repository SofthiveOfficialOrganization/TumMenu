using Application.Categories.Queries;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml.Linq;
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
    [Route("qr-kod")]
    public IActionResult BeQr()
    {
        return View();
    }

    [Route("ne-yesem")]
    public IActionResult WhatToEat()
    {
        return View();
    }

    [Route("hakkimizda")]
    public IActionResult About()
    {
        return View();
    }

    [Route("iletisim")]
    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }

    [Route("iletisim")]
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

    [Route("restoranlar")]
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

    [Route("yasal-bilgiler")]
    public IActionResult Legal()
    {
        return View();
    }

    [Route("fiyatlandirma")]
    public IActionResult Pricing()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // 301 Redirects for old URLs
    [Route("Home/About")]
    public IActionResult AboutRedirect()
    {
        return RedirectPermanent("/hakkimizda");
    }

    [Route("Home/Contact")]
    public IActionResult ContactRedirect()
    {
        return RedirectPermanent("/iletisim");
    }

    [Route("Home/Legal")]
    public IActionResult LegalRedirect()
    {
        return RedirectPermanent("/yasal-bilgiler");
    }

    [Route("Home/Pricing")]
    public IActionResult PricingRedirect()
    {
        return RedirectPermanent("/fiyatlandirma");
    }

    [Route("Home/Restaurants")]
    public IActionResult RestaurantsRedirect()
    {
        return RedirectPermanent("/restoranlar");
    }

    [Route("Home/BeQr")]
    public IActionResult BeQrRedirect()
    {
        return RedirectPermanent("/qr-kod");
    }

    [Route("Home/WhatToEat")]
    public IActionResult WhatToEatRedirect()
    {
        return RedirectPermanent("/ne-yesem");
    }

    [Route("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var urlset = new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"));
        
        // 1. Statik Sayfalar
        var staticPages = new[]
        {
            new { Url = "", ChangeFreq = "daily", Priority = "1.0" },
            new { Url = "/hakkimizda", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/iletisim", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/yasal-bilgiler", ChangeFreq = "monthly", Priority = "0.5" },
            new { Url = "/fiyatlandirma", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/restoranlar", ChangeFreq = "daily", Priority = "0.9" },
            new { Url = "/qr-kod", ChangeFreq = "monthly", Priority = "0.7" },
            new { Url = "/ne-yesem", ChangeFreq = "daily", Priority = "0.8" },
            new { Url = "/giris", ChangeFreq = "monthly", Priority = "0.6" },
            new { Url = "/kayit", ChangeFreq = "monthly", Priority = "0.6" },
            new { Url = "/sifremi-unuttum", ChangeFreq = "monthly", Priority = "0.4" },
            new { Url = "/hesabim", ChangeFreq = "weekly", Priority = "0.5" },
            new { Url = "/hesap-aktivasyon", ChangeFreq = "monthly", Priority = "0.3" }
        };

        foreach (var page in staticPages)
        {
            urlset.Add(new XElement(XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                new XElement(XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9"), $"{baseUrl}{page.Url}"),
                new XElement(XName.Get("lastmod", "http://www.sitemaps.org/schemas/sitemap/0.9"), DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new XElement(XName.Get("changefreq", "http://www.sitemaps.org/schemas/sitemap/0.9"), page.ChangeFreq),
                new XElement(XName.Get("priority", "http://www.sitemaps.org/schemas/sitemap/0.9"), page.Priority)
            ));
        }

        // 2. Dinamik Sayfalar (Şirketler, Kategoriler, Ürünler)
        var dynamicData = await mediator.Send(new Application.Sitemaps.Queries.GetSitemapDataQuery(), ct);

        foreach (var item in dynamicData.Items)
        {
            string url = item.Type switch
            {
                Application.Sitemaps.Queries.SitemapItemType.Store => $"/{item.CompanySlug}/{item.StoreSlug}",
                Application.Sitemaps.Queries.SitemapItemType.Category => $"/{item.CompanySlug}/{item.StoreSlug}/{item.CategorySlug}",
                Application.Sitemaps.Queries.SitemapItemType.Product => $"/{item.CompanySlug}/{item.StoreSlug}/{item.CategorySlug}/{item.ProductSlug}",
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(url)) continue;

            string priority = item.Type switch
            {
                Application.Sitemaps.Queries.SitemapItemType.Store => "0.9",
                Application.Sitemaps.Queries.SitemapItemType.Category => "0.8",
                Application.Sitemaps.Queries.SitemapItemType.Product => "0.6",
                _ => "0.5"
            };

            string changefreq = item.Type switch
            {
                Application.Sitemaps.Queries.SitemapItemType.Store => "daily",
                _ => "weekly"
            };

            urlset.Add(new XElement(XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                new XElement(XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9"), $"{baseUrl}{url}"),
                new XElement(XName.Get("lastmod", "http://www.sitemaps.org/schemas/sitemap/0.9"), item.LastModified.ToString("yyyy-MM-dd")),
                new XElement(XName.Get("changefreq", "http://www.sitemaps.org/schemas/sitemap/0.9"), changefreq),
                new XElement(XName.Get("priority", "http://www.sitemaps.org/schemas/sitemap/0.9"), priority)
            ));
        }

        var sitemap = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            urlset
        );

        return Content(sitemap.ToString(), "application/xml");
    }
}
