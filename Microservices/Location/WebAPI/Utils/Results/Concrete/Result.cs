using WebAPI.Exceptions.HttpProblemDetails;

namespace WebAPI.Utils.Results.Concrete;

public class Result : WebAPI.Utils.Results.Abstract.IResult
{
    public Result(bool success, string message, BaseProblemDetail errorModel) : this(success)
    {
        Message = message;
        ErrorModel = errorModel;
    }
    public Result(bool success)
    {
        Success = success;
    }
    public bool Success { get; }

    public string Message { get; }

    public BaseProblemDetail ErrorModel { get; }
}
