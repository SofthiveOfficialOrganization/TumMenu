using System.Text.Json.Serialization;
using WebAPI.Exceptions.HttpProblemDetails;

namespace WebAPI.Utils.Results.Abstract;

public interface IResult
{
    [JsonPropertyName("success")]
    bool Success { get; }

    [JsonPropertyName("message")]
    string? Message { get; }

    [JsonPropertyName("errorModel")]
    BaseProblemDetail? ErrorModel { get; }
}
