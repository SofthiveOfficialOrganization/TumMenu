using Application.Categories.Queries;
using Application.SystemSettings.Queries;
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
        return Redirect("/yasal-bilgiler#privacy");
    }
    [Route("qr-kod")]
    public IActionResult BeQr()
    {
        return View();
    }

    [Route("ozellikler")]
    public IActionResult Features()
    {
        return View();
    }

    [Route("sss")]
    public IActionResult Faq()
    {
        return View();
    }

    [Route("guncelleniyor")]
    public IActionResult Maintenance()
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        Response.Headers.RetryAfter = "120";
        return View();
    }

    [Route("restoran-kaynaklari")]
    public IActionResult Resources()
    {
        return View();
    }

    [Route("qr-menu-uygunluk-testi")]
    public IActionResult QrMenuReadinessTest()
    {
        return View();
    }

    [Route("menu-baski-maliyeti-hesaplayici")]
    public IActionResult MenuPrintingCostCalculator()
    {
        return View();
    }

    [Route("qr-menu-kurulum-kontrol-listesi")]
    public IActionResult QrMenuSetupChecklist()
    {
        return View();
    }

    [Route("restoran-menu-fotografi-rehberi")]
    public IActionResult RestaurantMenuPhotographyGuide()
    {
        return View();
    }

    [Route("kafe-dijital-menu-rehberi")]
    public IActionResult CafeDigitalMenuGuide()
    {
        return View();
    }

    [Route("restoran-online-menu-seo-rehberi")]
    public IActionResult RestaurantOnlineMenuSeoGuide()
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
    public async Task<IActionResult> Contact(CancellationToken ct)
    {
        await LoadLegalVersionsAsync(ct);
        return View(new ContactViewModel());
    }

    [Route("iletisim")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel model, CancellationToken ct)
    {
        var legalVersions = await mediator.Send(new GetLegalVersionSettingsQuery(), ct);
        ViewBag.LegalVersions = legalVersions;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var acceptedAtUtc = DateTime.UtcNow;
            var acceptedIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var acceptedUserAgent = Request.Headers.UserAgent.ToString();

            var htmlMessage = $@"
                <h3>İletişim Formundan Yeni Mesaj</h3>
                <p><strong>Ad Soyad:</strong> {model.Name}</p>
                <p><strong>E-posta:</strong> {model.Email}</p>
                <p><strong>Konu:</strong> {model.Subject}</p>
                <p><strong>Aydınlatma Onayı:</strong> Evet</p>
                <p><strong>Gizlilik Sürümü:</strong> {legalVersions.PrivacyVersion}</p>
                <p><strong>KVKK Sürümü:</strong> {legalVersions.KvkkVersion}</p>
                <p><strong>Onay Zamanı (UTC):</strong> {acceptedAtUtc:yyyy-MM-dd HH:mm:ss}</p>
                <p><strong>IP:</strong> {acceptedIp}</p>
                <p><strong>User-Agent:</strong> {acceptedUserAgent}</p>
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
            ViewBag.LegalVersions = legalVersions;
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
    public async Task<IActionResult> Legal(CancellationToken ct)
    {
        var legalVersions = await mediator.Send(new GetLegalVersionSettingsQuery(), ct);
        return View(legalVersions);
    }

    [Route("fiyatlandirma")]
    public IActionResult Pricing()
    {
        return StatusCode(StatusCodes.Status410Gone);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task LoadLegalVersionsAsync(CancellationToken ct)
    {
        ViewBag.LegalVersions = await mediator.Send(new GetLegalVersionSettingsQuery(), ct);
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
        return StatusCode(StatusCodes.Status410Gone);
    }

    [Route("Home/Features")]
    public IActionResult FeaturesRedirect()
    {
        return RedirectPermanent("/ozellikler");
    }

    [Route("Home/Faq")]
    public IActionResult FaqRedirect()
    {
        return RedirectPermanent("/sss");
    }

    [Route("Home/Resources")]
    public IActionResult ResourcesRedirect()
    {
        return RedirectPermanent("/restoran-kaynaklari");
    }

    [Route("Home/QrMenuReadinessTest")]
    public IActionResult QrMenuReadinessTestRedirect()
    {
        return RedirectPermanent("/qr-menu-uygunluk-testi");
    }

    [Route("Home/MenuPrintingCostCalculator")]
    public IActionResult MenuPrintingCostCalculatorRedirect()
    {
        return RedirectPermanent("/menu-baski-maliyeti-hesaplayici");
    }

    [Route("Home/QrMenuSetupChecklist")]
    public IActionResult QrMenuSetupChecklistRedirect()
    {
        return RedirectPermanent("/qr-menu-kurulum-kontrol-listesi");
    }

    [Route("Home/RestaurantMenuPhotographyGuide")]
    public IActionResult RestaurantMenuPhotographyGuideRedirect()
    {
        return RedirectPermanent("/restoran-menu-fotografi-rehberi");
    }

    [Route("Home/CafeDigitalMenuGuide")]
    public IActionResult CafeDigitalMenuGuideRedirect()
    {
        return RedirectPermanent("/kafe-dijital-menu-rehberi");
    }

    [Route("Home/RestaurantOnlineMenuSeoGuide")]
    public IActionResult RestaurantOnlineMenuSeoGuideRedirect()
    {
        return RedirectPermanent("/restoran-online-menu-seo-rehberi");
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
            new { Url = "/ozellikler", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/sss", ChangeFreq = "monthly", Priority = "0.7" },
            new { Url = "/restoran-kaynaklari", ChangeFreq = "weekly", Priority = "0.8" },
            new { Url = "/qr-menu-uygunluk-testi", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/menu-baski-maliyeti-hesaplayici", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/qr-menu-kurulum-kontrol-listesi", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/restoran-menu-fotografi-rehberi", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/kafe-dijital-menu-rehberi", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/restoran-online-menu-seo-rehberi", ChangeFreq = "monthly", Priority = "0.8" },
            new { Url = "/restoranlar", ChangeFreq = "daily", Priority = "0.9" },
            new { Url = "/qr-kod", ChangeFreq = "monthly", Priority = "0.7" },
            new { Url = "/ne-yesem", ChangeFreq = "daily", Priority = "0.8" },
            new { Url = "/blog", ChangeFreq = "daily", Priority = "0.9" }
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
                Application.Sitemaps.Queries.SitemapItemType.BlogPost => $"/blog/{item.BlogSlug}",
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(url)) continue;

            string priority = item.Type switch
            {
                Application.Sitemaps.Queries.SitemapItemType.Store => "0.9",
                Application.Sitemaps.Queries.SitemapItemType.Category => "0.8",
                Application.Sitemaps.Queries.SitemapItemType.Product => "0.6",
                Application.Sitemaps.Queries.SitemapItemType.BlogPost => "0.8",
                _ => "0.5"
            };

            string changefreq = item.Type switch
            {
                Application.Sitemaps.Queries.SitemapItemType.Store => "daily",
                Application.Sitemaps.Queries.SitemapItemType.BlogPost => "weekly",
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
