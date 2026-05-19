using Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using WebUI.Filters;
using WebUI.Infrastructure;
using WebUI.Services.SystemLogs;

namespace WebUI.IntegrationTests.Unit.Filters;

public class AppExceptionFilterTests
{
    [Fact]
    public async Task OnExceptionAsync_AdminRequest_ReturnsAdminErrorView()
    {
        var filter = CreateFilter(environmentName: Environments.Production);
        var context = CreateExceptionContext("/Admin/QRManagement", area: "Admin");
        context.Exception = new NullReferenceException("Object reference test message");

        await filter.OnExceptionAsync(context);

        var result = Assert.IsType<ViewResult>(context.Result);
        Assert.Equal("~/Areas/Admin/Views/Shared/Error.cshtml", result.ViewName);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.HttpContext.Response.StatusCode);
        Assert.Equal("Bir hata oluştu, işlemi lütfen tekrar deneyin.", result.ViewData["Message"]);
        Assert.True(context.ExceptionHandled);
    }

    [Fact]
    public async Task OnExceptionAsync_ProductUpdateValidation_ReturnsEditView()
    {
        var filter = CreateFilter(environmentName: Environments.Production);
        var context = CreateExceptionContext("/Admin/Product/Update", area: "Admin");
        context.RouteData.Values["controller"] = "Product";
        context.RouteData.Values["action"] = "Update";
        context.Exception = new ValidationAppException(new Dictionary<string, string[]>
        {
            ["BasePrice"] = ["Base price is too high"]
        });

        await filter.OnExceptionAsync(context);

        var result = Assert.IsType<ViewResult>(context.Result);
        Assert.Equal("Edit", result.ViewName);
		Assert.Equal(StatusCodes.Status200OK, context.HttpContext.Response.StatusCode);
        Assert.True(context.ExceptionHandled);
    }

    [Fact]
    public async Task OnExceptionAsync_AdminRequest_StashesOriginalArea()
    {
        var filter = CreateFilter(environmentName: Environments.Production);
        var context = CreateExceptionContext("/Admin/QRManagement", area: "Admin");
        context.Exception = new NullReferenceException("Object reference test message");

        await filter.OnExceptionAsync(context);

        var (area, path) = AdminRouteDetector.GetStashed(context.HttpContext);
        Assert.Equal("Admin", area);
        Assert.Equal("/Admin/QRManagement", path);
    }

    [Fact]
    public async Task OnExceptionAsync_PublicRequest_ReturnsPublicErrorView()
    {
        var filter = CreateFilter(environmentName: Environments.Production);
        var context = CreateExceptionContext("/menu", area: null);
        context.Exception = new InvalidOperationException("Public test message");

        await filter.OnExceptionAsync(context);

        var result = Assert.IsType<ViewResult>(context.Result);
        Assert.Equal("~/Views/Shared/Error.cshtml", result.ViewName);
    }

    private static AppExceptionFilter CreateFilter(string environmentName)
    {
        var logger = Mock.Of<ILogger<AppExceptionFilter>>();
        var environment = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(environmentName);

        var systemLogWriter = new Mock<ISystemLogWriter>();
        systemLogWriter
            .Setup(x => x.WriteExceptionAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<Exception?>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new AppExceptionFilter(logger, environment.Object, systemLogWriter.Object);
    }

    private static ExceptionContext CreateExceptionContext(string path, string? area)
    {
        var services = new ServiceCollection();
        var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
        tempDataFactory
            .Setup(x => x.GetTempData(It.IsAny<HttpContext>()))
            .Returns(Mock.Of<ITempDataDictionary>());
        services.AddSingleton(tempDataFactory.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        httpContext.Request.Path = path;

        var routeData = new RouteData();
        routeData.Values["controller"] = "QRManagement";
        routeData.Values["action"] = "Index";
        if (area is not null)
        {
            routeData.Values["area"] = area;
        }

        var actionContext = new ActionContext(
            httpContext,
            routeData,
            new ActionDescriptor());

        var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());
        if (area is not null)
            exceptionContext.RouteData.Values["area"] = area;

        return exceptionContext;
    }
}
