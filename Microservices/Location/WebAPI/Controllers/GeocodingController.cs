using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.Geocoding;
using WebAPI.Services.GeocodingServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GeocodingController(IGeocodingService geocodingService) : BaseMicroserviceController
{
    [ProducesResponseType(typeof(SuccessDataResult<List<GeocodingResultDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [SwaggerOperation(Summary = "Adres arama (Nominatim/OpenStreetMap)")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var results = await geocodingService.SearchAsync(q, limit, ct);
        return Ok(new SuccessDataResult<List<GeocodingResultDto>>(results));
    }

    [ProducesResponseType(typeof(SuccessDataResult<GeocodingResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Koordinattan adres çözümleme (reverse geocoding)")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Reverse([FromQuery] double lat, [FromQuery] double lon, CancellationToken ct = default)
    {
        var result = await geocodingService.ReverseAsync(lat, lon, ct);
        return Ok(new SuccessDataResult<GeocodingResultDto?>(result));
    }
}
