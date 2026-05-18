using Microsoft.AspNetCore.Http;

namespace WebUI.Security;

public static class SecurityRequestClassifier
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

    private static readonly HashSet<string> SuspiciousExactPaths = new(PathComparer)
    {
        "/.env",
        "/.aws/credentials",
        "/api/.env",
        "/app/.env",
        "/app/env",
        "/aws/credentials",
        "/backend/.env",
        "/backend/env",
        "/config/.env",
        "/laravel/.env"
    };

    private static readonly string[] SensitiveExtensions =
    [
        ".bak",
        ".sql",
        ".zip",
        ".tar",
        ".gz",
        ".config",
        ".yml",
        ".yaml",
        ".ini",
        ".log"
    ];

    private static readonly string[] AllowedSensitivePrefixPaths =
    [
        "/api/stores/search",
        "/api/categories/search",
        "/api/auth/login",
        "/owner/me",
        "/user/me"
    ];

    public static bool IsKnownSecurityProbe(PathString path)
    {
        var normalizedPath = NormalizePath(path.Value);
        return IsSecurityProbePath(normalizedPath);
    }

    public static SecurityRequestClassification ClassifySuccessfulRequest(HttpContext httpContext)
    {
        var normalizedPath = NormalizePath(httpContext.Request.Path.Value);

        if (IsSecurityProbePath(normalizedPath))
        {
            return new SecurityRequestClassification(
                SecurityRequestKind.SecurityProbeSuccess,
                "security_probe_success",
                "Şüpheli güvenlik tarama yolu başarılı yanıt aldı.");
        }

        if (IsUnexpectedSensitiveSuccess(httpContext, normalizedPath))
        {
            return new SecurityRequestClassification(
                SecurityRequestKind.UnexpectedSensitiveSuccess,
                "unexpected_sensitive_success",
                "Beklenmeyen hassas endpoint başarılı yanıt aldı.");
        }

        return SecurityRequestClassification.None;
    }

    private static bool IsSecurityProbePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            return false;
        }

        if (path.StartsWith("/.git", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (SuspiciousExactPaths.Contains(path))
        {
            return true;
        }

        return SensitiveExtensions.Any(extension =>
            path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUnexpectedSensitiveSuccess(HttpContext httpContext, string path)
    {
        if (IsHealthPath(path) || IsAllowedSensitivePrefixPath(path))
        {
            return false;
        }

        if (path.StartsWith("/api/location/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsPathOrChild(path, "/api"))
        {
            return true;
        }

        if (IsPathOrChild(path, "/owner") || IsPathOrChild(path, "/user"))
        {
            return true;
        }

        return false;
    }

    private static bool IsHealthPath(string path)
    {
        return path.Equals("/health", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAllowedSensitivePrefixPath(string path)
    {
        return AllowedSensitivePrefixPaths.Any(allowedPath =>
            path.Equals(allowedPath, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPathOrChild(string path, string prefix)
    {
        return path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var normalized = path;
        try
        {
            normalized = Uri.UnescapeDataString(normalized);
        }
        catch (UriFormatException)
        {
        }

        normalized = normalized.TrimEnd('/');
        return string.IsNullOrWhiteSpace(normalized) ? "/" : normalized;
    }
}
