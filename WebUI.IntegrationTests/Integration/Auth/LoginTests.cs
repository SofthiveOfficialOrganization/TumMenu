using Microsoft.AspNetCore.Mvc.Testing;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Auth;

public class LoginTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public LoginTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    [Fact]
    public async Task Login_WithValidOwnerCredentials_Redirects()
    {
        var client = CreateClient();
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/giris");

        var response = await client.PostAsync("/giris", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = TestDbSeeder.OwnerEmail,
            ["Input.Password"] = TestDbSeeder.OwnerPassword,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
            ["cf-turnstile-response"] = "test"
        }));

        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.Redirect ||
            response.StatusCode == System.Net.HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");
    }

    [Fact]
    public async Task Login_WithAdminDonusUrl_RedirectsToRequestedLocalUrl()
    {
        var client = CreateClient();
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/giris?DonusUrl=%2FAdmin%2FAdminSupport");

        var response = await client.PostAsync("/giris", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = TestDbSeeder.AdminEmail,
            ["Input.Password"] = TestDbSeeder.AdminPassword,
            ["Input.RememberMe"] = "false",
            ["DonusUrl"] = "/Admin/AdminSupport",
            ["__RequestVerificationToken"] = token,
            ["cf-turnstile-response"] = "test"
        }));

        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.Redirect ||
            response.StatusCode == System.Net.HttpStatusCode.Found,
            $"Expected redirect but got {response.StatusCode}");
        Assert.Equal("/Admin/AdminSupport", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsLoginPage()
    {
        var client = CreateClient();
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/giris");

        var response = await client.PostAsync("/giris", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = TestDbSeeder.OwnerEmail,
            ["Input.Password"] = "WrongPassword!",
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
            ["cf-turnstile-response"] = "test"
        }));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("giris", html.ToLower());
    }

    [Fact]
    public async Task Login_GetLoginPage_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/giris");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
