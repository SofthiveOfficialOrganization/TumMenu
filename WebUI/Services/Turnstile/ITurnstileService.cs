using WebUI.Models;

namespace WebUI.Services.Turnstile;

public interface ITurnstileService
{
    Task<TurnstileValidationResult> ValidateAsync(string? token);
}
