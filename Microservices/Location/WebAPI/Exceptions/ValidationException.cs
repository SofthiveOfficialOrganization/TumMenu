namespace WebAPI.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string BuildErrorMessage(IEnumerable<string> errors)
    {
        return $"Validation failed: {string.Join(Environment.NewLine, errors)}";
    }
}

public class ValidationExceptionModel
{
    public IEnumerable<string> Errors { get; set; }

    public ValidationExceptionModel(IEnumerable<string> errors)
    {
        Errors = errors;
    }
}
