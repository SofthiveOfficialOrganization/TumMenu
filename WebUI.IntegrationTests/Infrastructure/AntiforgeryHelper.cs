using System.Text.RegularExpressions;

namespace WebUI.IntegrationTests.Infrastructure;

public static class AntiforgeryHelper
{
    public static string ExtractToken(string html)
    {
        var match = Regex.Match(html,
            @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new InvalidOperationException(
                "Could not find __RequestVerificationToken in HTML response. " +
                "Make sure the page renders a form with antiforgery token.");

        return match.Groups[1].Value;
    }

    public static async Task<string> GetTokenAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return ExtractToken(html);
    }
}
