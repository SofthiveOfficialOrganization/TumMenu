using Newtonsoft.Json;

namespace WebAPI.Exceptions.HttpProblemDetails;

public abstract class BaseProblemDetail
{
    [JsonProperty("title")]
    public string Title { get; set; } = null!;
    [JsonProperty("detail")]
    public string Detail { get; set; } = null!;
    [JsonProperty("status")]
    public int Status { get; set; }
}