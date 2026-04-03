namespace WebUI.Models;

public class TurnstileValidationResult
{
    public bool Success { get; set; }
    public string? ChallengeTs { get; set; }
    public string? Hostname { get; set; }
    public double? Score { get; set; }
    public string? ScoreReason { get; set; }
    public List<string> ErrorCodes { get; set; } = new();
}
