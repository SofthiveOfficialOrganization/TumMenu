using System.Security.Claims;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Services.SystemLogs;

public sealed class SystemLogWriter(
    IServiceScopeFactory scopeFactory,
    ILogger<SystemLogWriter> logger) : ISystemLogWriter
{
    public async Task WriteExceptionAsync(
        HttpContext httpContext,
        Exception? exception,
        int statusCode,
        string? errorCode,
        string? responseMessage,
        string source,
        CancellationToken ct = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = httpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.Identity?.Name;

            var log = new SystemLog
            {
                Level = statusCode >= StatusCodes.Status500InternalServerError ? "Error" : "Warning",
                Source = source,
                StatusCode = statusCode,
                ErrorCode = errorCode,
                ResponseMessage = responseMessage,
                ExceptionType = exception?.GetType().FullName,
                ExceptionMessage = exception?.Message,
                StackTrace = exception?.ToString(),
                TraceId = httpContext.TraceIdentifier,
                HttpMethod = httpContext.Request.Method,
                Path = httpContext.Request.Path.Value,
                QueryString = httpContext.Request.QueryString.HasValue
                    ? httpContext.Request.QueryString.Value
                    : null,
                UserId = userId,
                UserName = userName,
                RemoteIp = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString()
            };

            log.Created(userId ?? "system");
            db.SystemLogs.Add(log);
            await db.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "System log could not be written. TraceId: {TraceId}", httpContext.TraceIdentifier);
        }
    }

    public async Task WriteSecurityEventAsync(
        HttpContext httpContext,
        int statusCode,
        string? errorCode,
        string? responseMessage,
        string source,
        CancellationToken ct = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = httpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.Identity?.Name;

            var log = new SystemLog
            {
                Level = "Warning",
                Source = source,
                StatusCode = statusCode,
                ErrorCode = errorCode,
                ResponseMessage = responseMessage,
                TraceId = httpContext.TraceIdentifier,
                HttpMethod = httpContext.Request.Method,
                Path = httpContext.Request.Path.Value,
                QueryString = httpContext.Request.QueryString.HasValue
                    ? httpContext.Request.QueryString.Value
                    : null,
                UserId = userId,
                UserName = userName,
                RemoteIp = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString()
            };

            log.Created(userId ?? "system");
            db.SystemLogs.Add(log);
            await db.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Security system log could not be written. TraceId: {TraceId}", httpContext.TraceIdentifier);
        }
    }
}
