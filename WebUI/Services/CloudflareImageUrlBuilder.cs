using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace WebUI.Services;

public sealed class CloudflareImageUrlBuilder : IImageUrlBuilder
{
    private const string TransformPrefix = "/cdn-cgi/image/";
    private const string TransformOptions = "format=auto,quality=75,metadata=none";

    private static readonly string[] TransformablePrefixes = ["/uploads/", "/images/"];
    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".svg", ".pdf", ".gif", ".ico"
    };

    private readonly bool _environmentEnabled;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public CloudflareImageUrlBuilder()
        : this(enabled: true)
    {
    }

    public CloudflareImageUrlBuilder(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        : this(!environment.IsDevelopment(), httpContextAccessor)
    {
    }

    private CloudflareImageUrlBuilder(bool enabled, IHttpContextAccessor? httpContextAccessor = null)
    {
        _environmentEnabled = enabled;
        _httpContextAccessor = httpContextAccessor;
    }

    public string Build(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return string.Empty;

        var trimmed = imageUrl.Trim();

        if (trimmed.StartsWith(TransformPrefix, StringComparison.OrdinalIgnoreCase))
            return trimmed;

        if (!ShouldUseTransform())
            return trimmed.StartsWith("~/", StringComparison.Ordinal)
                ? "/" + trimmed[2..].TrimStart('/')
                : trimmed;

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
            return trimmed;

        var normalized = trimmed.StartsWith("~/", StringComparison.Ordinal)
            ? "/" + trimmed[2..].TrimStart('/')
            : trimmed;

        if (!normalized.StartsWith("/", StringComparison.Ordinal))
            return trimmed;

        if (!TransformablePrefixes.Any(prefix => normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            return normalized;

        var path = normalized.Split('?', '#')[0];
        var extension = Path.GetExtension(path);
        if (ExcludedExtensions.Contains(extension))
            return normalized;

        return $"{TransformPrefix}{TransformOptions}/{normalized.TrimStart('/')}";
    }

    private bool ShouldUseTransform()
    {
        if (!_environmentEnabled)
            return false;

        var host = _httpContextAccessor?.HttpContext?.Request.Host.Host;
        return !IsLocalHost(host);
    }

    private static bool IsLocalHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
    }
}
