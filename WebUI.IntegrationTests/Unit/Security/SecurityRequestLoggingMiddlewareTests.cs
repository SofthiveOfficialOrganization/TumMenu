using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WebUI.Middleware;
using WebUI.Services.SystemLogs;

namespace WebUI.IntegrationTests.Unit.Security;

public class SecurityRequestLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SuspiciousPathWithSuccess_WritesSecurityEvent()
    {
        var writer = new Mock<ISystemLogWriter>();
        var context = CreateContext("/backup.sql");
        var middleware = CreateMiddleware(StatusCodes.Status200OK);

        await middleware.InvokeAsync(context, writer.Object);

        writer.Verify(x => x.WriteSecurityEventAsync(
            context,
            StatusCodes.Status200OK,
            "security_probe_success",
            It.IsAny<string>(),
            nameof(SecurityRequestLoggingMiddleware),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SuspiciousPathWithNotFound_DoesNotWriteSecurityEvent()
    {
        var writer = new Mock<ISystemLogWriter>();
        var context = CreateContext("/backup.sql");
        var middleware = CreateMiddleware(StatusCodes.Status404NotFound);

        await middleware.InvokeAsync(context, writer.Object);

        writer.Verify(x => x.WriteSecurityEventAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_NormalPublicSuccess_DoesNotWriteSecurityEvent()
    {
        var writer = new Mock<ISystemLogWriter>();
        var context = CreateContext("/restoranlar");
        var middleware = CreateMiddleware(StatusCodes.Status200OK);

        await middleware.InvokeAsync(context, writer.Object);

        writer.Verify(x => x.WriteSecurityEventAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_AllowedApiSuccess_DoesNotWriteSecurityEvent()
    {
        var writer = new Mock<ISystemLogWriter>();
        var context = CreateContext("/api/stores/search");
        var middleware = CreateMiddleware(StatusCodes.Status200OK);

        await middleware.InvokeAsync(context, writer.Object);

        writer.Verify(x => x.WriteSecurityEventAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SecurityRequestLoggingMiddleware CreateMiddleware(int statusCode)
    {
        return new SecurityRequestLoggingMiddleware(
            context =>
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            },
            NullLogger<SecurityRequestLoggingMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }
}
