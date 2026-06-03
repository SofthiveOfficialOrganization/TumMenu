using Application.Common.Errors;
using Application.Common.Exceptions;
using Application.Products.Commands;
using Application.Products.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebUI.Areas.Admin.Helpers;
using WebUI.Contracts;
using WebUI.Infrastructure;
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
				var (code, msg) = global::Infrastructure.Persistence.DbErrorTranslator.Translate(dbex);
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

		if(!wantsJson)
		{
			AdminRouteDetector.StashOriginalRequest(
				httpContext,
				context.RouteData.Values["area"]?.ToString(),
				httpContext.Request.Path.Value);
		}

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

		// In development mode, keep unexpected exceptions on the Developer Exception Page,
		// but still route translated database errors through TempData so form posts show toasts.
		if(_env.IsDevelopment() && !wantsJson && status >= 500 && ex is not DbUpdateException)
		{
			return;
		}

		if(wantsJson)
		{
			httpContext.Response.StatusCode = status;
			context.Result = new JsonResult(payload)
			{
				StatusCode = status
			};
			context.ExceptionHandled = true;
			return;
		}

		// --- MVC tarafı ---

		// Validation / Unprocessable → ModelState doldur, aynı action view'i göster (200 OK;
		// 4xx status would trigger StatusCodePages and replace the form with an error page)
		if(ex is ValidationAppException vax)
		{
			AddModelStateErrors(context, vax.Errors);
			httpContext.Response.StatusCode = StatusCodes.Status200OK;
			context.Result = await CreateCurrentActionViewResultAsync(context);
			context.ExceptionHandled = true;
			return;
		}

		if(ex is UnprocessableAppException uex)
		{
			if(uex.Errors is not null)
				AddModelStateErrors(context, uex.Errors);
			else
				context.ModelState.AddModelError(string.Empty, uex.Message);

			httpContext.Response.StatusCode = StatusCodes.Status200OK;
			context.Result = await CreateCurrentActionViewResultAsync(context);
			context.ExceptionHandled = true;
			return;
		}

		httpContext.Response.StatusCode = status;
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
			var (dbCode, dbMsg) = global::Infrastructure.Persistence.DbErrorTranslator.Translate(dbUpdateEx);
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

	private static async Task<ViewResult> CreateCurrentActionViewResultAsync(ExceptionContext context)
	{
		var controller = context.RouteData.Values["controller"]?.ToString();
		var actionName = context.RouteData.Values["action"]?.ToString();
		var viewName = ResolveActionViewName(controller, actionName);

		var viewData = new ViewDataDictionary(
			new EmptyModelMetadataProvider(),
			context.ModelState
		);

		if(TryGetReturnUrl(context, out var returnUrl))
			viewData["ReturnUrl"] = returnUrl;

		var model = await TryResolveValidationViewModelAsync(context, controller, actionName);
		if(model is not null)
			viewData.Model = model;

		var tempData = GetTempData(context.HttpContext);

		return new ViewResult
		{
			ViewName = viewName,
			ViewData = viewData,
			TempData = tempData
		};
	}

	private static string ResolveActionViewName(string? controller, string? actionName)
	{
		if(string.Equals(controller, "Product", StringComparison.OrdinalIgnoreCase) &&
		   string.Equals(actionName, "Update", StringComparison.OrdinalIgnoreCase))
			return "Edit";

		return actionName ?? string.Empty;
	}

	private static async Task<object?> TryResolveValidationViewModelAsync(
		ExceptionContext context,
		string? controller,
		string? actionName)
	{
		if(!string.Equals(controller, "Product", StringComparison.OrdinalIgnoreCase))
			return null;

		if(!string.Equals(actionName, "Edit", StringComparison.OrdinalIgnoreCase) &&
		   !string.Equals(actionName, "Update", StringComparison.OrdinalIgnoreCase))
			return null;

		if(!TryGetUpdateProductCommand(context, out var updateReq) ||
		   updateReq.Id == Guid.Empty)
			return null;

		var mediator = context.HttpContext.RequestServices.GetService<IMediator>();
		if(mediator is null)
			return null;

		try
		{
			return await ProductEditViewModelBuilder.BuildAsync(
				mediator,
				updateReq,
				context.HttpContext.RequestAborted);
		}
		catch
		{
			return null;
		}
	}

	private static bool TryGetUpdateProductCommand(ExceptionContext context, out UpdateProductCommand command)
	{
		if(context.HttpContext.Request.HasFormContentType)
		{
			var form = context.HttpContext.Request.Form;
			if(Guid.TryParse(form["Id"], out var id) && id != Guid.Empty)
			{
				command = new UpdateProductCommand
				{
					Id = id,
					Title = form["Title"].ToString() ?? string.Empty,
					Description = form["Description"].ToString(),
					CategoryId = Guid.TryParse(form["CategoryId"], out var categoryId) ? categoryId : Guid.Empty,
					BasePrice = decimal.TryParse(form["BasePrice"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var basePrice) ? basePrice : 0,
					SortOrder = int.TryParse(form["SortOrder"], out var sortOrder) ? sortOrder : 0,
					IsActive = form["IsActive"].Contains("true"),
					Allergens = form["Allergens"].ToString(),
					IsVegan = form["IsVegan"].Contains("true") ? true : null,
					IsVegetarian = form["IsVegetarian"].Contains("true") ? true : null,
					EstimatedPreparationTimeInMinutes = int.TryParse(form["EstimatedPreparationTimeInMinutes"], out var prep) ? prep : null,
					Prices = ParsePriceInputs(form)
				};
				return true;
			}
		}

		command = default!;
		return false;
	}

	private static List<ProductPriceInputDTO> ParsePriceInputs(IFormCollection form)
	{
		var prices = new List<ProductPriceInputDTO>();
		var index = 0;
		while(form.ContainsKey($"Prices[{index}].Size") || form.ContainsKey($"Prices[{index}].Price"))
		{
			decimal? price = decimal.TryParse(
				form[$"Prices[{index}].Price"],
				System.Globalization.NumberStyles.Any,
				System.Globalization.CultureInfo.InvariantCulture,
				out var parsedPrice)
				? parsedPrice
				: null;

			prices.Add(new ProductPriceInputDTO
			{
				Size = form[$"Prices[{index}].Size"].ToString(),
				Price = price
			});
			index++;
		}

		return prices;
	}

	private static bool TryGetReturnUrl(ExceptionContext context, out string? returnUrl)
	{
		if(context.HttpContext.Request.HasFormContentType)
		{
			returnUrl = context.HttpContext.Request.Form["returnUrl"].ToString();
			if(!string.IsNullOrEmpty(returnUrl))
				return true;
		}

		returnUrl = null;
		return false;
	}

	private static string ResolveErrorViewPath(ExceptionContext context)
	{
		var area = context.RouteData.Values["area"]?.ToString();
		var path = context.HttpContext.Request.Path.Value ?? string.Empty;

		return AdminRouteDetector.IsAdminRequest(area, path)
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
