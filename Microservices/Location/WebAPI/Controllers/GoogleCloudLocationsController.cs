using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Constants;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.Aws;
using WebAPI.Models.Dtos.GoogleCloud;
using WebAPI.Services.GoogleCloudServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GoogleCloudLocationsController(
    IGoogleCloudService googleCloudService
    ) : BaseMicroserviceController
{
    private readonly IGoogleCloudService _googleCloudService = googleCloudService;

    [ProducesResponseType(typeof(SuccessDataResult<LocationData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.GOOGLE_CLOUD_GET_COORDINATES)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetCoordinates([FromQuery] string address)
    {
        var location = await _googleCloudService.GetCoordinatesAsync(address);
        return Ok(new SuccessDataResult<LocationData>(data: location!));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<List<AddressSuggestionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.GOOOGLE_CLOUD_GET_ADDRESS_SUGGESTIONS)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAddressSuggestions([FromQuery] string query)
    {
        var location = await _googleCloudService.GetAddressSuggestionsAsync(query);

        return Ok(new SuccessDataResult<List<AddressSuggestionDto>>(data: location));
    }
}