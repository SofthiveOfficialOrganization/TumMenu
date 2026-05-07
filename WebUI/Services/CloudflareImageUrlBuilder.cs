using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace WebUI.Services;

public sealed class CloudflareImageUrlBuilder : IImageUrlBuilder
{
    private const string TransformPrefix = "/cdn-cgi/image/";
    private const string BaseTransformOptions = "format=auto,quality=75,metadata=none";

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

    public CloudflareImageUrlBuilder(IHttpContextAccessor httpContextAccessor)
        : this(enabled: true, httpContextAccessor)
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

    public string Build(
        string? imageUrl,
        int? width = null,
        int? height = null,
        ImageFitMode fit = ImageFitMode.Default,
        string? gravity = null)
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

        var transformOptions = BaseTransformOptions;
        if (width.HasValue) transformOptions += $",width={width.Value}";
        if (height.HasValue) transformOptions += $",height={height.Value}";
        if (fit == ImageFitMode.CoverCenter && width.HasValue && height.HasValue)
        {
            transformOptions += ",fit=cover";
            transformOptions += $",gravity={NormalizeGravity(gravity)}";
        }

        return $"{TransformPrefix}{transformOptions}/{normalized.TrimStart('/')}";
    }

    private static string NormalizeGravity(string? gravity)
    {
        return string.IsNullOrWhiteSpace(gravity)
            ? "0.5x0.5"
            : gravity.Trim();
    }

    private bool ShouldUseTransform()
    {
        if (!_environmentEnabled)
            return false;

        var context = _httpContextAccessor?.HttpContext;
        if (context is null)
            return true;

        var host = context.Request.Host.Host;
        return !IsLocalHost(host) && IsCloudflareRequest(context.Request);
    }

    private static bool IsCloudflareRequest(HttpRequest request)
    {
        return request.Headers.ContainsKey("CF-Ray")
            || request.Headers.ContainsKey("CF-Connecting-IP")
            || request.Headers.ContainsKey("CF-Visitor");
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
