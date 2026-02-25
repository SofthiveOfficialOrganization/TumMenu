using WebAPI.Utils.Results;

namespace WebAPI.Exceptions.HttpProblemDetails;

public class AuthorizationProblemDetails : BaseProblemDetail
{
    public AuthorizationProblemDetails(string detail)
    {
        Title = "Yetkilendirme Hatası";
        Detail = detail;
        Status = StatusCodes.Status401Unauthorized;
    }
}