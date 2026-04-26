using System.Text.Json;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Utils.Results.Concrete;

namespace WebAPI.Exceptions.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    public HttpResponse Response
    {
        get => _response ?? throw new NullReferenceException(nameof(_response));
        set => _response = value;
    }

    private HttpResponse? _response;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public override Task HandleException(BusinessException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        var result = new ErrorDataResult<BusinessProblemDetails>(errorModel: new BusinessProblemDetails(businessException.Message));
        return Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }

    public override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        var result = new ErrorDataResult<ValidationProblemDetails>(errorModel: new ValidationProblemDetails(validationException.Errors));
        return Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }

    public override Task HandleException(AuthorizationException authorizationException)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        var result = new ErrorDataResult<AuthorizationProblemDetails>(errorModel: new AuthorizationProblemDetails(authorizationException.Message));
        return Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }

    public override Task HandleException(NotFoundException notFoundException)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        var result = new ErrorDataResult<NotFoundProblemDetails>(errorModel: new NotFoundProblemDetails(notFoundException.Message));
        return Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }

    public override Task HandleException(System.Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        var result = new ErrorDataResult<InternalServerErrorProblemDetails>(errorModel: new InternalServerErrorProblemDetails(exception.Message));
        return Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
