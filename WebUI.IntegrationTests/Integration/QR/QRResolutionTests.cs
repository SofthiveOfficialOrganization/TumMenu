using System.Net;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.QR;

public class QRResolutionTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;
    private string _testQrKey = "testqr1";

    public QRResolutionTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
        SeedQrCodeAsync().GetAwaiter().GetResult();
    }

    private async Task SeedQrCodeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Remove if already exists to keep idempotent
        var existing = db.QRCodes.FirstOrDefault(q => q.PublicKey == _testQrKey);
        if (existing == null)
        {
            db.QRCodes.Add(new QRCode
            {
                PublicKey = _testQrKey,
                StoreId = TestDbSeeder.StoreId,
                ResolveMode = QRResolveMode.LatestActive,
                IsActive = true
            });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ResolveQR_WithValidKey_RedirectsToStore()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync($"/q/{_testQrKey}");

        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");

        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("isQr=true", location);
    }

    [Fact]
    public async Task ResolveQR_WithInvalidKey_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/q/doesnotexist999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResolveQR_RedirectUrlContainsStoreAndCompanySlug()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync($"/q/{_testQrKey}");
        var location = response.Headers.Location?.ToString();

        Assert.NotNull(location);
        Assert.Contains("test-sirketi", location);
        Assert.Contains("test-subesi", location);
    }
}
