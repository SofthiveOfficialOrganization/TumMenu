using System.Net;
using System.Net.Http.Json;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Admin;

public class SystemLogsAdminTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public SystemLogsAdminTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetSystemLogsIndex_AsAdmin_Returns200()
    {
        await SeedLog();
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Admin");

        var response = await client.GetAsync("/Admin/SystemLogs?TimeFrom=00:00&TimeTo=23:59");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSystemLogsIndex_AsOwner_ReturnsForbiddenOrRedirect()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");

        var response = await client.GetAsync("/Admin/SystemLogs");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 403 or redirect but got {response.StatusCode}");
    }

    [Fact]
    public async Task HandledWebUiException_WritesSystemLog()
    {
        var before = await CountOnboardingCompanyLogs();
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsJsonAsync("/admin/Onboarding/company", new
        {
            title = "Duplicate Company",
            slug = "duplicate-company"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var after = await CountOnboardingCompanyLogs();
        Assert.True(after > before, $"Expected a new SystemLog row, before={before}, after={after}");
    }

    private async Task SeedLog()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.SystemLogs.Add(new SystemLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)),
            CreatedBy = "test",
            Level = "Error",
            Source = "Test",
            StatusCode = 500,
            ResponseMessage = "Integration test log",
            ExceptionMessage = "Integration test log",
            TraceId = Guid.NewGuid().ToString("N"),
            HttpMethod = "GET",
            Path = "/test",
            UserName = TestDbSeeder.AdminEmail
        });
        await db.SaveChangesAsync();
    }

    private async Task<int> CountOnboardingCompanyLogs()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.SystemLogs.CountAsync(x =>
            x.Path == "/admin/Onboarding/company" &&
            x.StatusCode == 409 &&
            x.UserName == TestDbSeeder.OwnerEmail);
    }
}
