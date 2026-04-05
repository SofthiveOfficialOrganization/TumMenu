using System.Net;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Admin;

public class CompanyAdminTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public CompanyAdminTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetCompanyIndex_AsAdmin_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/Admin/Company/Index");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanyIndex_AsOwner_Returns200OrRedirect()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var client2 = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true
        });
        // Use the owner's cookies from AuthHelper
        var response = await client.GetAsync("/Admin/Company/Index");
        // Owner with existing company gets redirected to Details
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Unexpected: {response.StatusCode}");
    }

    [Fact]
    public async Task GetCompanyDetails_WithSeededCompanyId_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Admin");
        var response = await client.GetAsync($"/Admin/Company/Details?id={TestDbSeeder.CompanyId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanyIndex_Unauthenticated_Redirects()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/Admin/Company/Index");
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");
    }
}
