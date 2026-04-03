using System.Net.Mime;
using System.Text.Json;
using WebAPI.Exceptions.Handlers;
using WebAPI.Logging;
using WebAPI.Logging.Providers;

namespace WebAPI.Exceptions;

public class ExceptionMiddleware(
    RequestDelegate next, 
    IHttpContextAccessor contextAccessor,
    LoggerServiceBase loggerService
    )
{
    private readonly RequestDelegate _next = next;
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
    private readonly HttpExceptionHandler _httpExceptionHandler = new();
    private readonly LoggerServiceBase _loggerService = loggerService;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (global::FluentValidation.ValidationException exception)
        {
            await LogException(context, exception);
            
            var errorMessages = exception.Errors.Select(x => x.ErrorMessage);
            var validationException = new ValidationException(errors: errorMessages);
            await HandleExceptionAsync(context.Response, validationException);
        }
        catch (System.Exception exception)
        {
            await LogException(context, exception);
            await HandleExceptionAsync(context.Response, exception);
        }
    }

    protected virtual Task HandleExceptionAsync(HttpResponse response, dynamic exception)
    {
        response.ContentType = MediaTypeNames.Application.Json;
        _httpExceptionHandler.Response = response;

        return _httpExceptionHandler.HandleException(exception);
    }
    
    private Task LogException(HttpContext context, Exception exception)
    {
        List<LogParameter> logParameters =
            new() {
                new LogParameter { Type = context.GetType().Name, Value = exception.ToString() }
            };

        LogDetail logDetail =
            new() {
                MethodName = _next.Method?.Name ?? "Unknown",
                Parameters = logParameters,
                User = _contextAccessor.HttpContext?.User.Identity?.Name ?? "?"
            };

        _loggerService.Info(JsonSerializer.Serialize(logDetail));
        return Task.CompletedTask;
    }
}