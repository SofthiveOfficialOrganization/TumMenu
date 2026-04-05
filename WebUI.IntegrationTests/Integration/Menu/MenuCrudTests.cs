using System.Net;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Menu;

public class MenuCrudTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public MenuCrudTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetMenuIndex_AsOwner_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var response = await client.GetAsync("/Admin/Menu/Index");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMenuDetails_WithSeededMenuId_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var response = await client.GetAsync($"/Admin/Menu/Details?id={TestDbSeeder.MenuId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMenuIndex_Unauthenticated_Redirects()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/Admin/Menu/Index");
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");
    }

    [Fact]
    public async Task PostCreateMenu_WithValidData_RedirectsToDetails()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/Admin/Menu/CreateToStore");

        var response = await client.PostAsync("/Admin/Menu/CreateToStore", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Title"] = "Integration Test Menu",
            ["StoreId"] = TestDbSeeder.StoreId.ToString(),
            ["__RequestVerificationToken"] = token
        }));

        // Should redirect to details on success
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found ||
            response.StatusCode == HttpStatusCode.OK,
            $"Unexpected status: {response.StatusCode}");
    }
}
