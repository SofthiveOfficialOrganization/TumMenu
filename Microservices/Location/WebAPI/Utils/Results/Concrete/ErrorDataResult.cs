using WebAPI.Exceptions.HttpProblemDetails;

namespace WebAPI.Utils.Results.Concrete;

public class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(T data, BaseProblemDetail errorModel) : base(data, false, default, errorModel)
    {

    }
    public ErrorDataResult(T data) : base(data, false)
    {

    }
    public ErrorDataResult(BaseProblemDetail errorModel) : base(default, false, default, errorModel)
    {

    }
    public ErrorDataResult() : base(default, false)
    {

    }
}
