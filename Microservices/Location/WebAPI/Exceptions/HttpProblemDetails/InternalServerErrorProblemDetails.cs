using WebAPI.Utils.Results;

namespace WebAPI.Exceptions.HttpProblemDetails;

public class InternalServerErrorProblemDetails : BaseProblemDetail
{
    public InternalServerErrorProblemDetails(string detail)
    {
        Title = "İç Sunucu Hatası";
        Detail = "İşlemi daha sonra tekrar deneyin.";
        Status = StatusCodes.Status500InternalServerError;
    }
}
