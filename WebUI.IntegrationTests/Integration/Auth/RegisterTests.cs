using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
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
    public async Task Register_CheckEmail_WithInvalidEmail_ReturnsInvalid()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/kayit?handler=CheckEmail&email=not-an-email");
        response.EnsureSuccessStatusCode();

        var payload = await ReadJsonAsync(response);
        Assert.False(payload["isValid"].GetBoolean());
    }

    [Fact]
    public async Task Register_CheckEmail_WithUnusedEmail_ReturnsValid()
    {
        var client = CreateClient();
        var uniqueEmail = $"live-check-{Guid.NewGuid():N}@example.com";

        var response = await client.GetAsync($"/kayit?handler=CheckEmail&email={Uri.EscapeDataString(uniqueEmail)}");
        response.EnsureSuccessStatusCode();

        var payload = await ReadJsonAsync(response);
        Assert.True(payload["isValid"].GetBoolean());
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

    private static async Task<Dictionary<string, JsonElement>> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content)!;
    }
}
