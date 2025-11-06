using System.Net;
using Application.Common.Errors;
using Application.Common.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TumMenu.Contracts;

namespace TumMenu.Middleware;

public sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env) : IMiddleware
{
	private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
	private readonly IWebHostEnvironment _env = env;

	public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
	{
		try
		{
			await next(ctx);
		}
		catch(Exception ex)
		{
			await HandleAsync(ctx, ex);
		}
	}

	private async Task HandleAsync(HttpContext ctx, Exception ex)
	{
		var traceId = ctx.TraceIdentifier;

		ApiError payload;
		int status;

		switch(ex)
		{
			case ValidationAppException vex:
				status = (int)HttpStatusCode.BadRequest;
				payload = new ApiError { Status = status, Code = vex.Code, Message = vex.Message, TraceId = traceId, Errors = vex.Errors };
				break;

			case NotFoundAppException nfex:
				status = (int)HttpStatusCode.NotFound;
				payload = new ApiError { Status = status, Code = nfex.Code, Message = nfex.Message, TraceId = traceId };
				break;

			case ConflictAppException cfex:
				status = (int)HttpStatusCode.Conflict;
				payload = new ApiError { Status = status, Code = cfex.Code, Message = cfex.Message, TraceId = traceId };
				break;

			case ForbiddenAppException fbex:
				status = (int)HttpStatusCode.Forbidden;
				payload = new ApiError { Status = status, Code = fbex.Code, Message = fbex.Message, TraceId = traceId };
				break;

			case UnauthorizedAppException uaex:
				status = (int)HttpStatusCode.Unauthorized;
				payload = new ApiError { Status = status, Code = uaex.Code, Message = uaex.Message, TraceId = traceId };
				break;

			case ValidationException fvxex:
				status = (int)HttpStatusCode.BadRequest;
				var errors = fvxex.Errors
					.GroupBy(e => e.PropertyName)
					.ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

				payload = new ApiError { Status = status, Code = ErrorCodes.Validation, Message = "Validation error", TraceId = traceId, Errors = errors };
				break;

			case DbUpdateException dbex:
				var (code, msg) = Infrastructure.Persistence.DbErrorTranslator.Translate(dbex);
				status = code == ErrorCodes.DbDuplicate || code == ErrorCodes.DbForeignKey
					? (int)HttpStatusCode.Conflict
					: (int)HttpStatusCode.InternalServerError;

				payload = new ApiError { Status = status, Code = code, Message = msg, TraceId = traceId };
				break;

			default:
				status = (int)HttpStatusCode.InternalServerError;
				payload = new ApiError
				{
					Status = status,
					Code = ErrorCodes.Unknown,
					Message = _env.IsDevelopment() ? ex.Message : "Beklenmeyen bir hata oluştu.",
					TraceId = traceId,
					Details = _env.IsDevelopment() ? [ex.ToString()] : null
				};
				break;
		}

		if(status >= 500) _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
		else _logger.LogWarning(ex, "Handled exception. TraceId: {TraceId}", traceId);

		ctx.Response.ContentType = "application/json";
		ctx.Response.StatusCode = status;
		await ctx.Response.WriteAsJsonAsync(payload);
	}
}
