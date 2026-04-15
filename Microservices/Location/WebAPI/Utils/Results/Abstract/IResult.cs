using Newtonsoft.Json;
using WebAPI.Exceptions.HttpProblemDetails;

namespace WebAPI.Utils.Results.Abstract;

public interface IResult
{
    [JsonProperty("success")]
    bool Success { get; }

    [JsonProperty("message")]
    string? Message { get; }
    [JsonProperty("errorModel")]
    BaseProblemDetail? ErrorModel { get; }
}
