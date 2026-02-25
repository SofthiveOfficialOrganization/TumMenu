using Newtonsoft.Json;

namespace WebAPI.Exceptions.HttpProblemDetails;

public abstract class BaseProblemDetail
{
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("detail")]
    public string Detail { get; set; }
    [JsonProperty("status")]
    public int Status { get; set; }
}