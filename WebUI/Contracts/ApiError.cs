namespace WebUI.Contracts;

public sealed class ApiError
{
    public int Status { get; init; }
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public string TraceId { get; init; } = "";
    public IDictionary<string, string[]>? Errors { get; init; }
    public string[]? Details { get; init; }
}
