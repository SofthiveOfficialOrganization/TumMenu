using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebUI.Contracts;
using WebUI.Services.SystemLogs;

namespace WebUI.Controllers;

public sealed class ErrorController(
	IWebHostEnvironment env,
	ISystemLogWriter systemLogWriter) : Controller
{
	private readonly IWebHostEnvironment _env = env;
	private readonly ISystemLogWriter _systemLogWriter = systemLogWriter;

	[Route("error")]
	public async Task<IActionResult> Error(CancellationToken ct)
	{
		var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
		var originalPath = feature?.Path ?? string.Empty;
		var ex = feature?.Error;
		var statusCode = StatusCodes.Status500InternalServerError;
		var responseMessage = _env.IsDevelopment() ? ex?.Message ?? "Error" : "Beklenmeyen bir hata oluştu.";

		await _systemLogWriter.WriteExceptionAsync(
			HttpContext,
			ex,
			statusCode,
			"unknown",
			responseMessage,
			nameof(ErrorController),
			ct);

		if(IsApiRequest(HttpContext, originalPath))
		{
			var payload = new ApiError
			{
				Status = statusCode,
				Code = "unknown",
				Message = responseMessage,
				TraceId = HttpContext.TraceIdentifier
			};

			return new JsonResult(payload) { StatusCode = payload.Status };
		}

		Response.StatusCode = statusCode;
		var message = _env.IsDevelopment()
			? ex?.Message ?? "Beklenmeyen bir hata oluştu."
			: "Bir hata oluştu, işlemi lütfen tekrar deneyin.";
		ViewData["Message"] = message;
		ViewData["TraceId"] = HttpContext.TraceIdentifier;
		TempData["Error"] = message;

		return View(ResolveViewPath(originalPath, adminPath: "Error", publicPath: "Error"));
	}

	[Route("status-code/{code:int}")]
	public IActionResult StatusCodePage(int code)
	{
		var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
		var originalPath = feature?.OriginalPath ?? string.Empty;

		if(IsApiRequest(HttpContext, originalPath))
		{
			var payload = new ApiError
			{
				Status = code,
				Code = code == StatusCodes.Status404NotFound ? "not_found" : "http_error",
				Message = code == StatusCodes.Status404NotFound ? "Bulunamadı" : "HTTP hatası",
				TraceId = HttpContext.TraceIdentifier
			};

			return new JsonResult(payload) { StatusCode = code };
		}

		Response.StatusCode = code;
		ViewData["Code"] = code;
		ViewData["TraceId"] = HttpContext.TraceIdentifier;

		if(code == StatusCodes.Status404NotFound)
			return View(ResolveViewPath(originalPath, adminPath: "NotFound", publicPath: "NotFound"));

		if(code == StatusCodes.Status400BadRequest)
		{
			ViewData["Message"] = "İstek güvenlik veya doğrulama kontrolünden geçemedi. Sayfayı yenileyip tekrar deneyebilirsiniz.";
			return View(ResolveViewPath(originalPath, adminPath: "BadRequest", publicPath: "BadRequest"));
		}

		var msg = code == StatusCodes.Status403Forbidden
			? "Bu sayfaya erişim yetkiniz bulunmuyor."
			: "İstek işlenemedi.";
		ViewData["Message"] = msg;
		TempData["Error"] = msg;
		return View(ResolveViewPath(originalPath, adminPath: "Error", publicPath: "Error"));
	}

	private string ResolveViewPath(string originalPath, string adminPath, string publicPath)
	{
		var isAdmin = originalPath.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);

		if(isAdmin)
		{
			var adminView = $"Areas/Admin/Views/Shared/{adminPath}.cshtml";
			if(_env.ContentRootFileProvider.GetFileInfo(adminView).Exists)
				return $"~/{adminView}";
		}

		return $"~/Views/Shared/{publicPath}.cshtml";
	}

	private static bool IsApiRequest(HttpContext ctx, string? originalPath = null)
	{
		if(IsApiPath(originalPath) || IsApiPath(ctx.Request.Path.Value))
			return true;

		var accept = ctx.Request.Headers.Accept.ToString();
		if(accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
			return true;

		var requestedWith = ctx.Request.Headers["X-Requested-With"].ToString();
		if(requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
			return true;

		return false;
	}

	private static bool IsApiPath(string? path)
	{
		if(string.IsNullOrWhiteSpace(path))
			return false;

		return IsPathOrChild(path, "/api") ||
		       IsPathOrChild(path, "/user") ||
		       IsPathOrChild(path, "/owner");
	}

	private static bool IsPathOrChild(string path, string prefix)
	{
		return path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
		       path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
	}
}
