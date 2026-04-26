using System.Text.Json.Serialization;

namespace WebAPI.Exceptions.HttpProblemDetails;

public abstract class BaseProblemDetail
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = null!;

    [JsonPropertyName("status")]
    public int Status { get; set; }
}
