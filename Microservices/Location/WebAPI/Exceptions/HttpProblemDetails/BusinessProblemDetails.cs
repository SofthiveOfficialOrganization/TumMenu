using WebAPI.Utils.Results;

namespace WebAPI.Exceptions.HttpProblemDetails;

public class BusinessProblemDetails : BaseProblemDetail
{
    public BusinessProblemDetails(string detail)
    {
        Title = "Kural İhlali";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest;
    }
}