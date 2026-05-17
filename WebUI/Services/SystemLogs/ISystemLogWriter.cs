namespace WebUI.Services.SystemLogs;

public interface ISystemLogWriter
{
    Task WriteExceptionAsync(
        HttpContext httpContext,
        Exception? exception,
        int statusCode,
        string? errorCode,
        string? responseMessage,
        string source,
        CancellationToken ct = default);
}
