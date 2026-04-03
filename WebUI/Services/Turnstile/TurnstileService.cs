using System.Text.Json;
using WebUI.Models;

namespace WebUI.Services.Turnstile;

public class TurnstileService : ITurnstileService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _secretKey;
    private readonly bool _enabled;
    private const string VerificationUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public TurnstileService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        var turnstileConfig = configuration.GetSection("CloudflareTurnstile");
        _secretKey = turnstileConfig["SecretKey"] ?? string.Empty;
        _enabled = turnstileConfig.GetValue<bool>("Enabled", true);
    }

    public async Task<TurnstileValidationResult> ValidateAsync(string? token)
    {
        // If Turnstile is disabled (for development/testing), always succeed
        if (!_enabled)
        {
            return new TurnstileValidationResult { Success = true };
        }

        // Token is required
        if (string.IsNullOrWhiteSpace(token))
        {
            return new TurnstileValidationResult
            {
                Success = false,
                ErrorCodes = new List<string> { "missing-token" }
            };
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var requestBody = new { secret = _secretKey, response = token };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(VerificationUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TurnstileValidationResult>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new TurnstileValidationResult
            {
                Success = false,
                ErrorCodes = new List<string> { "deserialization-error" }
            };
        }
        catch (Exception ex)
        {
            // Log the exception (assuming ILogger is injected)
            return new TurnstileValidationResult
            {
                Success = false,
                ErrorCodes = new List<string> { "validation-error", ex.Message }
            };
        }
    }
}
