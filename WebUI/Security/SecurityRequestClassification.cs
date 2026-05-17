namespace WebUI.Security;

public enum SecurityRequestKind
{
    None = 0,
    SecurityProbeSuccess = 1,
    UnexpectedSensitiveSuccess = 2
}

public sealed record SecurityRequestClassification(
    SecurityRequestKind Kind,
    string? ErrorCode,
    string? Message)
{
    public static SecurityRequestClassification None { get; } = new(
        SecurityRequestKind.None,
        null,
        null);
}
