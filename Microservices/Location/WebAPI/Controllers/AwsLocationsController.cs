using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Constants;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.Aws;
using WebAPI.Models.Dtos.District;
using WebAPI.Services.AwsServices;
using WebAPI.Utils.Results.Concrete;
using AwsResDto = WebAPI.Models.Dtos.Aws.AwsResDto;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AwsLocationsController(
    IAwsService awsService
    ) : BaseMicroserviceController
{
    private readonly IAwsService _awsService = awsService;
    
    [ProducesResponseType(typeof(SuccessDataResult<List<LocationData>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.AWS_GET_COORDINATES)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetCoordinates([FromQuery] string address)
    {
        var result = await _awsService.GetCoordinatesAsync(address);

        return Ok(new SuccessDataResult<List<LocationData>?>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<List<AwsAddressSuggestionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.AWS_GET_ADDRESS_SUGGESTIONS)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAddressSuggestions([FromQuery] string query)
    {
        var location = await _awsService.GetAddressSuggestionsAsync(query);

        return Ok(new SuccessDataResult<List<AwsAddressSuggestionDto>>(data:location));
    }
}