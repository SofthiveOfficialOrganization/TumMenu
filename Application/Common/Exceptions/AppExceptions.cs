namespace Application.Common.Exceptions;

public abstract class AppException : Exception
{
	public string Code { get; }
	protected AppException(string code, string message) : base(message) => Code = code;
}

public sealed class ValidationAppException : AppException
{
	public IDictionary<string, string[]> Errors { get; }
	public ValidationAppException(IDictionary<string, string[]> errors)
		: base(Common.Errors.ErrorCodes.Validation, "Validation error") => Errors = errors;
}

public sealed class NotFoundAppException : AppException
{
	public NotFoundAppException(string message) : base(Errors.ErrorCodes.NotFound, message) { }
}

public sealed class ConflictAppException : AppException
{
	public ConflictAppException(string message) : base(Errors.ErrorCodes.Conflict, message) { }
}

public sealed class ForbiddenAppException : AppException
{
	public ForbiddenAppException(string message) : base(Errors.ErrorCodes.Forbidden, message) { }
}

public sealed class UnauthorizedAppException : AppException
{
	public UnauthorizedAppException(string message) : base(Errors.ErrorCodes.Unauthorized, message) { }
}

public sealed class AlreadyExistsAppException : AppException
{
	public AlreadyExistsAppException(string message) : base(Errors.ErrorCodes.AlreadyExists, message) { }
}