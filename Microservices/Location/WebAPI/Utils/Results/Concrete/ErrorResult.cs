using WebAPI.Exceptions.HttpProblemDetails;

namespace WebAPI.Utils.Results.Concrete;

public class ErrorResult : Result
{
    public ErrorResult(string message, BaseProblemDetail errorModel) : base(false, message, errorModel)
    {

    }
    public ErrorResult() : base(false)
    {

    }
}
