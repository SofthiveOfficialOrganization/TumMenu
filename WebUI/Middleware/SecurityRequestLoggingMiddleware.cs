using WebUI.Security;
using WebUI.Services.SystemLogs;

namespace WebUI.Middleware;

public sealed class SecurityRequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<SecurityRequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ISystemLogWriter systemLogWriter)
    {
        await next(context);

        if (context.Response.StatusCode < StatusCodes.Status200OK ||
            context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return;
        }

        var classification = SecurityRequestClassifier.ClassifySuccessfulRequest(context);
        if (classification.Kind == SecurityRequestKind.None)
        {
            return;
        }

        try
        {
            await systemLogWriter.WriteSecurityEventAsync(
                context,
                context.Response.StatusCode,
                classification.ErrorCode,
                classification.Message,
                nameof(SecurityRequestLoggingMiddleware),
                context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Security request log could not be written. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
    }
}
