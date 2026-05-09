using Domain.Entities;
using Application.SystemSettings.Queries;
using Infrastructure.Persistence;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebUI.Extensions;

public static class SeedExtensions
{
    private static readonly MarkdownPipeline BlogMarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();


    public static async Task SeedAdminAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        await SeedSystemSettingsAsync(db);
        await SeedBlogPostsAsync(db, environment);
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

    private static async Task SeedBlogPostsAsync(ApplicationDbContext db, IWebHostEnvironment environment)
    {
        var existingSlugs = await db.BlogPosts
            .Select(b => b.Slug)
            .ToListAsync();

        var editorialPosts = EditorialBlogPostSeedData.GetPosts()
            .Concat(GetMarkdownBlogPosts(environment))
            .GroupBy(p => p.Slug, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();
        var editorialIds = editorialPosts
            .Select(p => p.Id)
            .ToHashSet();
        var editorialSlugs = editorialPosts
            .Select(p => p.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Match by both Id and Slug to make seeding idempotent even if slugs change over time.
        var existingEditorialPosts = await db.BlogPosts
            .Where(b => editorialSlugs.Contains(b.Slug) || editorialIds.Contains(b.Id))
            .ToListAsync();

        var hasEditorialUpdates = false;

        foreach (var seededPost in editorialPosts)
        {
            var existing = existingEditorialPosts.FirstOrDefault(p => p.Id == seededPost.Id)
                ?? existingEditorialPosts.FirstOrDefault(p =>
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
                existing.Slug = seededPost.Slug;
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

    private static List<BlogPost> GetMarkdownBlogPosts(IWebHostEnvironment environment)
    {
        var blogsDirectory = FindBlogsDirectory(environment);
        if (blogsDirectory is null)
        {
            return [];
        }

        return Directory
            .EnumerateFiles(blogsDirectory, "*.md", SearchOption.TopDirectoryOnly)
            .Select(TryReadMarkdownBlogPost)
            .Where(post => post is not null)
            .Cast<BlogPost>()
            .OrderByDescending(post => post.PublishedAt)
            .ThenBy(post => post.Slug)
            .ToList();
    }

    private static string? FindBlogsDirectory(IWebHostEnvironment environment)
    {
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "Blogs"),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "Blogs")),
            Path.Combine(AppContext.BaseDirectory, "Blogs")
        };

        return candidates.FirstOrDefault(Directory.Exists);
    }

    private static BlogPost? TryReadMarkdownBlogPost(string path)
    {
        var content = File.ReadAllText(path);
        var parsed = ParseMarkdownFrontMatter(content);

        if (!parsed.HasFrontMatter)
        {
            return null;
        }

        var title = GetRequiredFrontMatterValue(parsed.FrontMatter, "title", path);
        var slug = GetRequiredFrontMatterValue(parsed.FrontMatter, "slug", path);
        var summary = GetRequiredFrontMatterValue(parsed.FrontMatter, "summary", path);
        var publishedAtValue = GetRequiredFrontMatterValue(parsed.FrontMatter, "publishedAt", path);

        if (!DateTime.TryParse(
                publishedAtValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var publishedAt))
        {
            throw new InvalidOperationException($"Blog markdown publishedAt okunamadi: {path}");
        }

        var markdownBody = RemoveLeadingTitleHeading(parsed.Body, title);
        var html = Markdown.ToHtml(markdownBody, BlogMarkdownPipeline);

        return new BlogPost
        {
            Id = CreateDeterministicGuid($"markdown-blog:{slug}"),
            Title = title,
            Slug = slug,
            Summary = summary,
            Content = html,
            CoverImageUrl = GetOptionalFrontMatterValue(parsed.FrontMatter, "coverImageUrl"),
            PublishedAt = publishedAt.Date,
            IsPublished = ParseBooleanFrontMatter(parsed.FrontMatter, "isPublished", defaultValue: true),
            Tags = GetOptionalFrontMatterValue(parsed.FrontMatter, "tags"),
            CreatedAt = new DateTimeOffset(publishedAt.Date, TimeSpan.FromHours(3)),
            CreatedBy = "markdown-seed"
        };
    }

    private static ParsedMarkdown ParseMarkdownFrontMatter(string content)
    {
        var match = Regex.Match(content, @"\A---\r?\n(?<frontMatter>.*?)\r?\n---\r?\n?", RegexOptions.Singleline);
        if (!match.Success)
        {
            return new ParsedMarkdown(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), content, false);
        }

        var frontMatter = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var frontMatterText = match.Groups["frontMatter"].Value;
        foreach (var rawLine in frontMatterText.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var separatorIndex = line.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            frontMatter[key] = UnquoteFrontMatterValue(value);
        }

        var body = content[match.Length..];
        return new ParsedMarkdown(frontMatter, body, true);
    }

    private static string UnquoteFrontMatterValue(string value)
    {
        if (value.Length >= 2 &&
            ((value.StartsWith('"') && value.EndsWith('"')) ||
             (value.StartsWith('\'') && value.EndsWith('\''))))
        {
            return value[1..^1];
        }

        return value;
    }

    private static string GetRequiredFrontMatterValue(
        IReadOnlyDictionary<string, string> frontMatter,
        string key,
        string path)
    {
        var value = GetOptionalFrontMatterValue(frontMatter, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Blog markdown front matter eksik: {key} ({path})");
        }

        return value;
    }

    private static string? GetOptionalFrontMatterValue(IReadOnlyDictionary<string, string> frontMatter, string key)
    {
        if (!frontMatter.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value;
    }

    private static bool ParseBooleanFrontMatter(
        IReadOnlyDictionary<string, string> frontMatter,
        string key,
        bool defaultValue)
    {
        var value = GetOptionalFrontMatterValue(frontMatter, key);
        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static string RemoveLeadingTitleHeading(string markdown, string title)
    {
        var escapedTitle = Regex.Escape(title.Trim());
        return Regex.Replace(
            markdown,
            $@"\A\s*#\s+{escapedTitle}\s*\r?\n+",
            string.Empty,
            RegexOptions.IgnoreCase);
    }

    private static Guid CreateDeterministicGuid(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(bytes);
    }

    private sealed record ParsedMarkdown(
        Dictionary<string, string> FrontMatter,
        string Body,
        bool HasFrontMatter);

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
