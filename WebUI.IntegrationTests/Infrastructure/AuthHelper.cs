using System.Net;

namespace WebUI.IntegrationTests.Infrastructure;

public static class AuthHelper
{
    public static async Task<HttpClient> GetAuthenticatedClientAsync(
        TumMenuWebAppFactory factory, string role)
    {
        var (email, password) = role == "Admin"
            ? (TestDbSeeder.AdminEmail, TestDbSeeder.AdminPassword)
            : (TestDbSeeder.OwnerEmail, TestDbSeeder.OwnerPassword);

        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        // Get antiforgery token from login page
        var loginPage = await client.GetAsync("/giris");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = AntiforgeryHelper.ExtractToken(html);

        var loginData = new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
            ["cf-turnstile-response"] = "test-token"
        };

        var response = await client.PostAsync("/giris", new FormUrlEncodedContent(loginData));

        // Should redirect after successful login
        if (response.StatusCode != HttpStatusCode.Redirect &&
            response.StatusCode != HttpStatusCode.Found)
        {
            throw new InvalidOperationException(
                $"Login failed for {email}. Status: {response.StatusCode}. " +
                $"Body: {await response.Content.ReadAsStringAsync()}");
        }

        return client;
    }
}
