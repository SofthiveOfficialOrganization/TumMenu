namespace WebAPI.Utils.Results.Concrete;

public class SuccessResult : Result
{
    public SuccessResult(string message) : base(true, message, default)
    {

    }
    public SuccessResult() : base(true)
    {

    }
}
