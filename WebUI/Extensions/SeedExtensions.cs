using Domain.Entities;
using Application.SystemSettings.Queries;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Extensions;

public static class SeedExtensions
{


    public static async Task SeedAdminAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedSystemSettingsAsync(db);
        await SeedBlogPostsAsync(db);
        await SeedMenuDesignsAsync(db);

        string adminEmail = "yonetim@softhive.com";
        string adminPassword = "SoftHive123!";
        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new ApplicationRole(adminRole));
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            await userManager.AddToRoleAsync(admin, adminRole);
        }
    }

    private static async Task SeedMenuDesignsAsync(ApplicationDbContext db)
    {
        var seedDesigns = MenuDesignSeedData.GetDesigns();
        var existingDesigns = await db.MenuDesigns
            .ToListAsync();
        var existingSlugs = existingDesigns
            .Select(d => d.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toAdd = seedDesigns
            .Where(d => !existingSlugs.Contains(d.Slug))
            .ToList();
        var hasUpdates = false;

        foreach (var existing in existingDesigns)
        {
            var seed = seedDesigns.FirstOrDefault(d =>
                string.Equals(d.Slug, existing.Slug, StringComparison.OrdinalIgnoreCase));

            if (seed is null)
            {
                continue;
            }

            if (ShouldUpdateSeededAsset(
                existing.BackgroundImageUrl,
                seed.BackgroundImageUrl))
            {
                existing.BackgroundImageUrl = seed.BackgroundImageUrl;
                hasUpdates = true;
            }

            if (ShouldUpdateSeededAsset(
                existing.PreviewImageUrl,
                seed.PreviewImageUrl))
            {
                existing.PreviewImageUrl = seed.PreviewImageUrl;
                hasUpdates = true;
            }
        }

        if (toAdd.Count > 0 || hasUpdates)
        {
            db.MenuDesigns.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }

    private static bool ShouldUpdateSeededAsset(
        string? current,
        string? seeded)
    {
        if (string.IsNullOrWhiteSpace(seeded))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(current) ||
            current.StartsWith("https://images.unsplash.com/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task SeedBlogPostsAsync(ApplicationDbContext db)
    {
        var existingSlugs = await db.BlogPosts
            .Select(b => b.Slug)
            .ToListAsync();

        var editorialPosts = EditorialBlogPostSeedData.GetPosts();
        var editorialSlugs = editorialPosts
            .Select(p => p.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingEditorialPosts = await db.BlogPosts
            .Where(b => editorialSlugs.Contains(b.Slug))
            .ToListAsync();

        var hasEditorialUpdates = false;

        foreach (var seededPost in editorialPosts)
        {
            var existing = existingEditorialPosts.FirstOrDefault(p =>
                string.Equals(p.Slug, seededPost.Slug, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                db.BlogPosts.Add(seededPost);
                existingSlugs.Add(seededPost.Slug);
                hasEditorialUpdates = true;
                continue;
            }

            if (ShouldUpdateEditorialPost(existing, seededPost))
            {
                existing.Title = seededPost.Title;
                existing.Summary = seededPost.Summary;
                existing.Content = seededPost.Content;
                existing.CoverImageUrl = seededPost.CoverImageUrl;
                existing.PublishedAt = seededPost.PublishedAt;
                existing.IsPublished = seededPost.IsPublished;
                existing.Tags = seededPost.Tags;
                existing.ModifiedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
                existing.ModifiedBy = "seed";
                hasEditorialUpdates = true;
            }
        }

        var removedLegacyPosts = await RemoveLegacyAdSenseSeedPostsAsync(db);

        if (hasEditorialUpdates || removedLegacyPosts)
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task<bool> RemoveLegacyAdSenseSeedPostsAsync(ApplicationDbContext db)
    {
        var legacySlugs = GetLegacyAdSenseSeedBlogSlugs();
        var legacyPosts = await db.BlogPosts
            .Where(p => legacySlugs.Contains(p.Slug))
            .ToListAsync();

        var hasUpdates = false;

        foreach (var post in legacyPosts)
        {
            if (post.IsDeleted && !post.IsPublished)
            {
                continue;
            }

            post.IsPublished = false;
            post.Deleted("seed");
            hasUpdates = true;
        }

        return hasUpdates;
    }

    private static string[] GetLegacyAdSenseSeedBlogSlugs() =>
    [
        "2025te-restoran-teknoloji-tren",
        "fast-food-vs-ev-yemegi-hangisi",
        "glutensiz-secenekler-neden-one",
        "icecek-menusu-tasarim-ipuclari",
        "kahvalti-menusu-icin-populer-s",
        "kucuk-kafeler-icin-qr-menu-ava",
        "menu-fiyatlandirma-stratejiler",
        "mevsimlik-malzeme-kullaniminin",
        "musteri-deneyimini-qr-menu-ile",
        "musteri-sadakati-nasil-saglani",
        "online-siparise-hazirlik-rehbe",
        "personel-egitiminde-dikkat-edi",
        "qr-menu-ile-kagit-menu-karsila",
        "qr-menu-kurulum-rehberi-adim-a",
        "qr-menude-fotograf-kullanimini",
        "restoran-acmadan-once-bilmeniz",
        "restoran-hijyen-standartlari",
        "restoran-menusu-nasil-tasarlan",
        "restoranlar-neden-dijital-menu",
        "sezonluk-menu-guncelleme-ipucl",
        "sosyal-medyada-restoran-taniti",
        "tatli-menusu-nasil-olusturulur",
        "turk-mutfaginin-vazgecilmez-le",
        "vejetaryen-menu-olusturma-rehb"
    ];

    private static bool ShouldUpdateEditorialPost(BlogPost existing, BlogPost seeded)
    {
        return existing.Title != seeded.Title ||
            existing.Summary != seeded.Summary ||
            existing.Content != seeded.Content ||
            existing.CoverImageUrl != seeded.CoverImageUrl ||
            existing.PublishedAt != seeded.PublishedAt ||
            existing.IsPublished != seeded.IsPublished ||
            existing.Tags != seeded.Tags;
    }

    private static async Task SeedSystemSettingsAsync(ApplicationDbContext db)
    {
        var trackedSettings = await db.SystemSettings
            .Where(s =>
                s.Type == SystemSettingType.LegalVersionTerms ||
                s.Type == SystemSettingType.LegalVersionKvkk ||
                s.Type == SystemSettingType.LegalVersionPrivacy ||
                s.Type == SystemSettingType.LegalVersionCookie)
            .ToListAsync();

        var defaults = GetLegalVersionSettingsQueryHandler.GetDefaults();
        var toAdd = defaults
            .Where(d => trackedSettings.All(s => s.Type != d.Type))
            .Select(d => new SystemSetting
            {
                Type = d.Type,
                Value = d.Value,
                Description = d.Description
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            db.SystemSettings.AddRange(toAdd);
            await db.SaveChangesAsync();
        }
    }
}
