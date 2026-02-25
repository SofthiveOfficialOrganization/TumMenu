using WebAPI.Utils.Results;

namespace WebAPI.Exceptions.HttpProblemDetails;

public class NotFoundProblemDetails : BaseProblemDetail
{
    public NotFoundProblemDetails(string detail)
    {
        Title = "Bulunamadı";
        Detail = detail;
        Status = StatusCodes.Status404NotFound;
    }
}
