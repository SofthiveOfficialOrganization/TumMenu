using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Utils.Results.Abstract;

namespace WebAPI.Utils.Results.Concrete;

public class DataResult<T> : Result, IDataResult<T>
{
    public DataResult(T data, bool success, string message, BaseProblemDetail errorModel) : base(success, message, errorModel)
    {
        Data = data;
    }
    public DataResult(T data, bool success) : base(success)
    {
        Data = data;
    }
    public T Data { get; }
}
