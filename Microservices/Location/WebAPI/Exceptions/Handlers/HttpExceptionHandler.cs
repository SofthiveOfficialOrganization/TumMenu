using Newtonsoft.Json;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Utils.Results.Concrete;

namespace WebAPI.Exceptions.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    public HttpResponse Response
    {
        #pragma warning disable S112 // General or reserved exceptions should never be thrown
        get => _response ?? throw new NullReferenceException(nameof(_response));
        #pragma warning restore S112 // General or reserved exceptions should never be thrown
        set => _response = value;
    }

    private HttpResponse? _response;

    public override Task HandleException(BusinessException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        var details = new BusinessProblemDetails(businessException.Message);
        var errorDataResult = new ErrorDataResult<BusinessProblemDetails>(errorModel: details);
        var jsonResponse = JsonConvert.SerializeObject(errorDataResult, Formatting.Indented);

        return Response.WriteAsync(jsonResponse);
    }

    public override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        var details = new ValidationProblemDetails(validationException.Errors);
        var errorDataResult = new ErrorDataResult<ValidationProblemDetails>(errorModel: details);
        string jsonResponse = JsonConvert.SerializeObject(errorDataResult, Formatting.Indented);

        return Response.WriteAsync(jsonResponse);
    }

    public override Task HandleException(AuthorizationException authorizationException)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        var details = new AuthorizationProblemDetails(authorizationException.Message);
        var errorDataResult = new ErrorDataResult<AuthorizationProblemDetails>(errorModel: details);
        var jsonResponse = JsonConvert.SerializeObject(errorDataResult, Formatting.Indented);

        return Response.WriteAsync(jsonResponse);
    }

    public override Task HandleException(NotFoundException notFoundException)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        var details = new NotFoundProblemDetails(notFoundException.Message);
        var errorDataResult = new ErrorDataResult<NotFoundProblemDetails>(errorModel: details);
        var jsonResponse = JsonConvert.SerializeObject(errorDataResult, Formatting.Indented);

        return Response.WriteAsync(jsonResponse);
    }

    public override Task HandleException(System.Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        var details = new InternalServerErrorProblemDetails(exception.Message);
        var errorDataResult = new ErrorDataResult<InternalServerErrorProblemDetails>(errorModel: details);
        var jsonResponse = JsonConvert.SerializeObject(errorDataResult, Formatting.Indented);

        return Response.WriteAsync(jsonResponse);
    }
}