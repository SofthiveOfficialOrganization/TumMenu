using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.Seo;

/// <summary>Public sayfa SEO yardımcıları (routing ve dinamik slug yapısına dokunmaz).</summary>
public static class PublicSeo
{
    public const string DefaultOgImagePath = "/images/tum-menu-logo-safe.webp";
    public const string DefaultCanonicalBaseUrl = "https://tummenu.com";

    public static bool IsIndexable(string? robotsContent) =>
        string.IsNullOrWhiteSpace(robotsContent)
        || !robotsContent.Contains("noindex", StringComparison.OrdinalIgnoreCase);

    public static string GetCanonicalBaseUrl(HttpContext? context = null)
    {
        var configured = context?.RequestServices
            .GetService<IConfiguration>()?["Seo:CanonicalBaseUrl"];

        var normalizedConfigured = string.IsNullOrWhiteSpace(configured)
            ? null
            : configured.Trim().TrimEnd('/');

        if (Uri.TryCreate(normalizedConfigured, UriKind.Absolute, out var configuredUri))
        {
            return configuredUri.ToString().TrimEnd('/');
        }

        return DefaultCanonicalBaseUrl;
    }

    public static string ToCanonicalUrl(HttpRequest request, PathString path, QueryString queryString = default)
    {
        var baseUrl = GetCanonicalBaseUrl(request.HttpContext);
        var pathValue = path.HasValue ? path.Value : "/";
        var normalizedPath = string.IsNullOrWhiteSpace(pathValue) ? "/" : pathValue;

        return $"{baseUrl}{normalizedPath}{queryString}";
    }

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

        return $"{GetCanonicalBaseUrl(request.HttpContext)}{path}";
    }

    public static string ResolveOgImage(HttpRequest request, string? viewDataOgImage) =>
        string.IsNullOrWhiteSpace(viewDataOgImage)
            ? ToAbsoluteUrl(request, DefaultOgImagePath)
            : ToAbsoluteUrl(request, viewDataOgImage);
}
