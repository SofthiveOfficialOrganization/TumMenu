namespace WebAPI.Utils.Results.Concrete;

public class SuccessDataResult<T> : DataResult<T>
{

    public SuccessDataResult(T data, string message) : base(data, true, message, default)
    {

    }
    public SuccessDataResult(T data) : base(data, true)
    {

    }
    public SuccessDataResult(string message) : base(default, true, message, default)
    {

    }
    public SuccessDataResult() : base(default, true)
    {

    }
}
