using System.Net;
using System.Net.Http.Json;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Onboarding;

public class OnboardingFlowTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public OnboardingFlowTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetOnboardingPage_AsOwner_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var response = await client.GetAsync("/admin/Onboarding");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOnboardingPage_Unauthenticated_Redirects()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/admin/Onboarding");
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");
    }

    [Fact]
    public async Task CreateCompany_WhenAlreadyExists_Returns409()
    {
        // Seeded owner already has a company — second create should be rejected.
        // The AppExceptionFilter returns 409 Conflict for AlreadyExistsAppException when
        // the request accepts JSON; without JSON accept it redirects to referer.
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.PostAsJsonAsync("/admin/Onboarding/company", new
        {
            title = "Another Company",
            slug = "another-company"
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateMenu_WithValidStoreId_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var response = await client.PostAsJsonAsync("/admin/Onboarding/menu", new
        {
            title = "New Test Menu",
            storeId = TestDbSeeder.StoreId
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
