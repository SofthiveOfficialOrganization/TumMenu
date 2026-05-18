using Application.Common.Errors;
using Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebUI.Contracts;
using WebUI.Models;
using WebUI.Services.SystemLogs;

namespace WebUI.Filters;

public sealed class AppExceptionFilter(
	ILogger<AppExceptionFilter> logger,
	IWebHostEnvironment env,
	ISystemLogWriter systemLogWriter
) : IAsyncExceptionFilter
{
	private readonly ILogger<AppExceptionFilter> _logger = logger;
	private readonly IWebHostEnvironment _env = env;
	private readonly ISystemLogWriter _systemLogWriter = systemLogWriter;

	public Task OnExceptionAsync(ExceptionContext context)
	{
		var httpContext = context.HttpContext;
		var ex = context.Exception;
		var traceId = httpContext.TraceIdentifier;

		var wantsJson = IsApiRequest(httpContext);

		int status;
		ApiError payload;

		switch(ex)
		{
			case ValidationAppException vex:
				status = StatusCodes.Status400BadRequest;
				payload = new ApiError
				{
					Status = status,
					Code = vex.Code,
					Message = vex.Message,
					TraceId = traceId,
					Errors = vex.Errors
				};
				return Handle(context, wantsJson, status, payload, ex);

			case UnprocessableAppException uex:
				status = StatusCodes.Status422UnprocessableEntity;
				payload = new ApiError
				{
					Status = status,
					Code = uex.Code,
					Message = uex.Message,
					TraceId = traceId,
					Errors = uex.Errors
				};
				return Handle(context, wantsJson, status, payload, ex);

			case NotFoundAppException nfex:
				status = StatusCodes.Status404NotFound;
				payload = new ApiError
				{
					Status = status,
					Code = nfex.Code,
					Message = nfex.Message,
					TraceId = traceId
				};
				return Handle(context, wantsJson, status, payload, ex, isNotFound: true);

			case ConflictAppException cfex:
				status = StatusCodes.Status409Conflict;
				payload = new ApiError
				{
					Status = status,
					Code = cfex.Code,
					Message = cfex.Message,
					TraceId = traceId
				};
				return Handle(context, wantsJson, status, payload, ex);

			case ForbiddenAppException fbex:
				status = StatusCodes.Status403Forbidden;
				payload = new ApiError
				{
					Status = status,
					Code = fbex.Code,
					Message = fbex.Message,
					TraceId = traceId
				};
				return Handle(context, wantsJson, status, payload, ex);

			case UnauthorizedAppException uaex:
				status = StatusCodes.Status401Unauthorized;
				payload = new ApiError
				{
					Status = status,
					Code = uaex.Code,
					Message = uaex.Message,
					TraceId = traceId
				};
				return Handle(context, wantsJson, status, payload, ex);

			case AlreadyExistsAppException aex:
				status = StatusCodes.Status409Conflict;
				payload = new ApiError
				{
					Status = status,
					Code = aex.Code,
					Message = aex.Message,
					TraceId = traceId
				};
				return Handle(context, wantsJson, status, payload, ex);

			case ValidationException fvxex:
				status = StatusCodes.Status400BadRequest;

				var errors = fvxex.Errors
					.GroupBy(e => e.PropertyName)
					.ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage).Distinct().ToArray()
					);

				payload = new ApiError
				{
					Status = status,
					Code = ErrorCodes.Validation,
					Message = "Doğrulama hatası",
					TraceId = traceId,
					Errors = errors
				};
				return Handle(context, wantsJson, status, payload, ex);

			case DbUpdateException dbex:
				var (code, msg) = Infrastructure.Persistence.DbErrorTranslator.Translate(dbex);
				status = code == ErrorCodes.DbDuplicate || code == ErrorCodes.DbForeignKey
					? StatusCodes.Status409Conflict
					: StatusCodes.Status500InternalServerError;

				payload = new ApiError
				{
					Status = status,
					Code = code,
					Message = msg,
					TraceId = traceId
				};
				return Handle(context, wantsJson, status, payload, ex);

			default:
				status = StatusCodes.Status500InternalServerError;
				payload = new ApiError
				{
					Status = status,
					Code = ErrorCodes.Unknown,
					Message = _env.IsDevelopment() ? ex.Message : "Beklenmeyen bir hata oluştu.",
					TraceId = traceId,
					Details = _env.IsDevelopment() ? [ex.ToString()] : null
				};
				return Handle(context, wantsJson, status, payload, ex);
		}
	}

	private async Task Handle(
		ExceptionContext context,
		bool wantsJson,
		int status,
		ApiError payload,
		Exception ex,
		bool isNotFound = false
	)
	{
		var httpContext = context.HttpContext;
		var traceId = httpContext.TraceIdentifier;

		if(status >= 500)
			_logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
		else
			_logger.LogWarning(ex, "Handled exception. TraceId: {TraceId}", traceId);

		await _systemLogWriter.WriteExceptionAsync(
			httpContext,
			ex,
			status,
			payload.Code,
			payload.Message,
			nameof(AppExceptionFilter),
			httpContext.RequestAborted);

		// In development mode, allow unhandled exceptions or DB exceptions to bubble up to the Developer Exception Page
		if(_env.IsDevelopment() && !wantsJson && (status >= 500 || ex is DbUpdateException))
		{
			return;
		}

		httpContext.Response.StatusCode = status;

		if(wantsJson)
		{
			context.Result = new JsonResult(payload)
			{
				StatusCode = status
			};
			context.ExceptionHandled = true;
			return;
		}

		// --- MVC tarafı ---

		// Validation / Unprocessable → ModelState doldur, aynı action view'i göster
		if(ex is ValidationAppException vax)
		{
			AddModelStateErrors(context, vax.Errors);
			context.Result = CreateCurrentActionViewResult(context);
			context.ExceptionHandled = true;
			return;
		}

		if(ex is UnprocessableAppException uex)
		{
			if(uex.Errors is not null)
				AddModelStateErrors(context, uex.Errors);
			else
				context.ModelState.AddModelError(string.Empty, uex.Message);

			context.Result = CreateCurrentActionViewResult(context);
			context.ExceptionHandled = true;
			return;
		}
		if(ex is AlreadyExistsAppException aex)
		{
			var aexTempData = GetTempData(httpContext);
			aexTempData["Error"] = aex.Message;

			var referer = httpContext.Request.Headers["Referer"].ToString();
			if(!string.IsNullOrEmpty(referer))
				context.Result = new RedirectResult(referer);
			else
			{
				context.Result = new RedirectResult("/admin/Dashboard");
			}

			context.ExceptionHandled = true;
			return;
		}
		if(isNotFound)
		{
			context.Result = new EmptyResult();
			context.ExceptionHandled = true;
			return;
		}

		if(ex is DbUpdateException dbUpdateEx)
		{
			var (dbCode, dbMsg) = Infrastructure.Persistence.DbErrorTranslator.Translate(dbUpdateEx);
			var dbTempData = GetTempData(httpContext);
			dbTempData["Error"] = dbMsg;

			var dbReferer = httpContext.Request.Headers["Referer"].ToString();
			if(!string.IsNullOrEmpty(dbReferer))
				context.Result = new RedirectResult(dbReferer);
			else
			{
				var role = httpContext.User.IsInRole("Admin") ? "admin" : "owner";
				context.Result = new RedirectResult($"/admin/Dashboard");
			}

			context.ExceptionHandled = true;
			return;
		}

		// Diğerleri → Error view
		var errorModel = new ErrorViewModel
		{
			RequestId = traceId
		};

		var viewDataError = new ViewDataDictionary(
			new EmptyModelMetadataProvider(),
			context.ModelState
		)
		{
			Model = errorModel,
			["Message"] = _env.IsDevelopment()
				? ex.Message
				: "Bir hata oluştu, işlemi lütfen tekrar deneyin.",
			["TraceId"] = traceId
		};

		// Set TempData["Error"] for the toast notification
		var factory = httpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
		var tempData = factory.GetTempData(httpContext);
		tempData["Error"] = _env.IsDevelopment() ? ex.Message : "Bir hata oluştu, işlemi lütfen tekrar deneyin.";

		context.Result = new ViewResult
		{
			ViewName = ResolveErrorViewPath(context),
			ViewData = viewDataError,
			TempData = tempData
		};

		context.ExceptionHandled = true;
	}

	private static void AddModelStateErrors(ExceptionContext context, IDictionary<string, string[]> errors)
	{
		foreach(var (key, messages) in errors)
		{
			foreach(var message in messages)
			{
				context.ModelState.AddModelError(key, message);
			}
		}
	}

	private static ViewResult CreateCurrentActionViewResult(ExceptionContext context)
	{
		var actionName = context.RouteData.Values["action"]?.ToString();

		var viewData = new ViewDataDictionary(
			new EmptyModelMetadataProvider(),
			context.ModelState
		);

		var tempData = GetTempData(context.HttpContext);

		return new ViewResult
		{
			ViewName = actionName,
			ViewData = viewData,
			TempData = tempData
		};
	}

	private static string ResolveErrorViewPath(ExceptionContext context)
	{
		var area = context.RouteData.Values["area"]?.ToString();
		var path = context.HttpContext.Request.Path.Value ?? string.Empty;
		var isAdmin = area?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true ||
			path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);

		return isAdmin
			? "~/Areas/Admin/Views/Shared/Error.cshtml"
			: "~/Views/Shared/Error.cshtml";
	}


	private static bool IsApiRequest(HttpContext ctx)
	{
		if(ctx.Request.Path.StartsWithSegments("/api"))
			return true;

		var accept = ctx.Request.Headers["Accept"].ToString();
		if(accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
			return true;

		var requestedWith = ctx.Request.Headers["X-Requested-With"].ToString();
		if(requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
			return true;

		return false;
	}

	private static ITempDataDictionary GetTempData(HttpContext httpContext)
	{
		var factory = httpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
		return factory.GetTempData(httpContext);
	}
}
