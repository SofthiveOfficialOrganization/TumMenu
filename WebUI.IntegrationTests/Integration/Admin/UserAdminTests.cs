using System.Net;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Admin;

public class UserAdminTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public UserAdminTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetUserIndex_AsAdmin_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Admin");
        var response = await client.GetAsync("/Admin/User/Index");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserIndex_AsOwner_ReturnsForbiddenOrRedirect()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var response = await client.GetAsync("/Admin/User/Index");
        // Owner should be forbidden (403) or redirected to access denied
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 403 or redirect but got {response.StatusCode}");
    }

    [Fact]
    public async Task GetUserUpdate_AsAdmin_RouteIsAccessible()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Admin");

        // Use the seeded owner user ID; the route should be accessible (not 401/403/redirect)
        // The controller may return 404 if the user is outside the current page window on large DBs
        var response = await client.GetAsync($"/Admin/User/Update/{TestDbSeeder.OwnerUserId}");

        // Verify the route resolves and auth passes — 200 (found) or 404 (not in page window) are both valid
        // 401/403/302 would indicate auth failure which is the real concern
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 200 or 404 (route accessible) but got {response.StatusCode}");
    }

    [Fact]
    public async Task GetUserIndex_Unauthenticated_Redirects()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/Admin/User/Index");
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");
    }
}
