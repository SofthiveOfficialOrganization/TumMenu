using Application.BlogPosts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

public class BlogController(IMediator mediator) : Controller
{
    private static readonly Dictionary<string, string> LegacyRedirects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["2025te-restoran-teknoloji-tren"] = "restoran-teknolojileri-trendleri",
        ["kucuk-kafeler-icin-qr-menu-ava"] = "kucuk-kafeler-icin-qr-menu-avantajlari",
        ["menu-fiyatlandirma-stratejiler"] = "restoran-menu-fiyatlandirma-stratejileri",
        ["musteri-deneyimini-qr-menu-ile"] = "qr-menu-musteri-deneyimini-nasil-iyilestirir",
        ["online-siparise-hazirlik-rehbe"] = "online-siparise-hazirlik-rehberi",
        ["qr-menu-ile-kagit-menu-karsila"] = "qr-menu-ile-kagit-menu-karsilastirmasi",
        ["qr-menu-kurulum-rehberi-adim-a"] = "qr-menu-kurulumu-adim-adim-restoran-rehberi",
        ["qr-menude-fotograf-kullanimini"] = "menu-fotograflari-dijital-menude-neden-onemlidir",
        ["restoran-menusu-nasil-tasarlan"] = "restoran-menusu-nasil-tasarlanir",
        ["restoranlar-neden-dijital-menu"] = "restoranlar-neden-dijital-menuye-gecmeli",
        ["sosyal-medyada-restoran-taniti"] = "restoranlar-icin-sosyal-medya-menu-tanitimi"
    };

    private static readonly HashSet<string> LegacyGoneSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "fast-food-vs-ev-yemegi-hangisi",
        "glutensiz-secenekler-neden-one",
        "icecek-menusu-tasarim-ipuclari",
        "kahvalti-menusu-icin-populer-s",
        "mevsimlik-malzeme-kullaniminin",
        "musteri-sadakati-nasil-saglani",
        "personel-egitiminde-dikkat-edi",
        "restoran-acmadan-once-bilmeniz",
        "restoran-hijyen-standartlari",
        "sezonluk-menu-guncelleme-ipucl",
        "tatli-menusu-nasil-olusturulur",
        "turk-mutfaginin-vazgecilmez-le",
        "vejetaryen-menu-olusturma-rehb"
    };

    [HttpGet("/blog")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var posts = await mediator.Send(new GetAllBlogPostsPagedQuery
        {
            PageIndex = page,
            PageSize = 10,
            IsPublished = true
        }, ct);
        return View(posts);
    }

    [HttpGet("/blog/{slug}")]
    public async Task<IActionResult> Post(string slug, CancellationToken ct)
    {
        if (LegacyRedirects.TryGetValue(slug, out var redirectSlug))
        {
            return RedirectPermanent($"/blog/{redirectSlug}");
        }

        if (LegacyGoneSlugs.Contains(slug))
        {
            return StatusCode(StatusCodes.Status410Gone);
        }

        var post = await mediator.Send(new GetBlogPostBySlugQuery { Slug = slug }, ct);
        return View(post);
    }

    [HttpGet("/blog/etiket/{tag}")]
    public async Task<IActionResult> Tag(string tag, int page = 1, CancellationToken ct = default)
    {
        var posts = await mediator.Send(new GetBlogPostsByTagQuery
        {
            Tag = tag,
            PageIndex = page,
            PageSize = 10
        }, ct);
        ViewBag.Tag = tag;
        return View(posts);
    }
}
