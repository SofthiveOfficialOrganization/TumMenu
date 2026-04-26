using System.Text.Json.Serialization;

namespace WebAPI.Exceptions.HttpProblemDetails;

public class ValidationProblemDetails : BaseProblemDetail
{
    [JsonPropertyName("errors")]
    public IEnumerable<string> Errors { get; init; }

    public ValidationProblemDetails(IEnumerable<string> errors)
    {
        Title = "Doğrulama Hataları";
        Detail = "Bir veya daha fazla doğrulama hatası oluştu.";
        Errors = errors;
        Status = StatusCodes.Status422UnprocessableEntity;
    }
}
