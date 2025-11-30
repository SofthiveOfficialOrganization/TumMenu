using Application.Common.Errors;

namespace Application.Common.Exceptions;

public abstract class AppException : Exception
{
	public string Code { get; }

	protected AppException(string code, string message) : base(message)
		=> Code = code;
}

public sealed class ValidationAppException : AppException
{
	public IDictionary<string, string[]> Errors { get; }

	public ValidationAppException(IDictionary<string, string[]> errors)
		: base(ErrorCodes.Validation, "Validation error")
		=> Errors = errors;
}

public sealed class UnprocessableAppException : AppException
{
	public IDictionary<string, string[]>? Errors { get; }

	public UnprocessableAppException(string message, IDictionary<string, string[]>? errors = null)
		: base(ErrorCodes.Unprocessable, message)
		=> Errors = errors;
}

public sealed class NotFoundAppException : AppException
{
	public NotFoundAppException(string message)
		: base(ErrorCodes.NotFound, message) { }
}

public sealed class ConflictAppException : AppException
{
	public ConflictAppException(string message)
		: base(ErrorCodes.Conflict, message) { }
}

public sealed class ForbiddenAppException : AppException
{
	public ForbiddenAppException(string message)
		: base(ErrorCodes.Forbidden, message) { }
}

public sealed class UnauthorizedAppException : AppException
{
	public UnauthorizedAppException(string message)
		: base(ErrorCodes.Unauthorized, message) { }
}

public sealed class AlreadyExistsAppException : AppException
{
	public AlreadyExistsAppException(string message)
		: base(ErrorCodes.AlreadyExists, message) { }
}
