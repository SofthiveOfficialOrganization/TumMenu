using Microsoft.AspNetCore.Http;

namespace WebUI.Seo;

/// <summary>Public sayfa SEO yardımcıları (routing ve dinamik slug yapısına dokunmaz).</summary>
public static class PublicSeo
{
    public const string DefaultOgImagePath = "/images/tum-menu-logo-safe.webp";

    public static bool IsIndexable(string? robotsContent) =>
        string.IsNullOrWhiteSpace(robotsContent)
        || !robotsContent.Contains("noindex", StringComparison.OrdinalIgnoreCase);

    public static string ToAbsoluteUrl(HttpRequest request, string? urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath))
        {
            return ToAbsoluteUrl(request, DefaultOgImagePath);
        }

        var trimmed = urlOrPath.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        var path = trimmed.StartsWith("~/", StringComparison.Ordinal)
            ? "/" + trimmed[2..].TrimStart('/')
            : trimmed.StartsWith('/') ? trimmed : "/" + trimmed;

        return $"{request.Scheme}://{request.Host}{path}";
    }

    public static string ResolveOgImage(HttpRequest request, string? viewDataOgImage) =>
        string.IsNullOrWhiteSpace(viewDataOgImage)
            ? ToAbsoluteUrl(request, DefaultOgImagePath)
            : ToAbsoluteUrl(request, viewDataOgImage);
}
