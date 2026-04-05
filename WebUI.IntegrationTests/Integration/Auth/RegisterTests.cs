using Microsoft.AspNetCore.Mvc.Testing;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Auth;

public class RegisterTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public RegisterTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    [Fact]
    public async Task Register_GetPage_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/kayit");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithPasswordMismatch_ReturnsRegisterPage()
    {
        var client = CreateClient();
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/kayit");

        var response = await client.PostAsync("/kayit", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = "newuser@example.com",
            ["Input.Password"] = "ValidPass123!",
            ["Input.ConfirmPassword"] = "DifferentPass123!",
            ["__RequestVerificationToken"] = token,
            ["cf-turnstile-response"] = "test"
        }));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidData_RedirectsOrConfirms()
    {
        var client = CreateClient();
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/kayit");
        var uniqueEmail = $"reg-test-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsync("/kayit", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = uniqueEmail,
            ["Input.Password"] = "ValidPass123!",
            ["Input.ConfirmPassword"] = "ValidPass123!",
            ["__RequestVerificationToken"] = token,
            ["cf-turnstile-response"] = "test"
        }));

        // Should redirect or show confirmation
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.Redirect ||
            response.StatusCode == System.Net.HttpStatusCode.Found ||
            response.StatusCode == System.Net.HttpStatusCode.OK,
            $"Unexpected status: {response.StatusCode}");
    }
}
