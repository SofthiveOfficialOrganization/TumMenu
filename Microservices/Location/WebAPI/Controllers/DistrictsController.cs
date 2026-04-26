using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.District;
using WebAPI.Services.DistrictServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DistrictsController(IDistrictService districtService) : BaseMicroserviceController
{
    [ProducesResponseType(typeof(SuccessDataResult<List<DistrictDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "İl ID'ye göre ilçeleri listeler")]
    [HttpGet("[action]/{provinceId}")]
    public async Task<IActionResult> GetListByProvinceId([FromRoute] Guid provinceId, CancellationToken ct = default)
    {
        var result = await districtService.GetListByProvinceIdAsync(provinceId, ct);
        return Ok(new SuccessDataResult<List<DistrictDto>>(result));
    }

    [ProducesResponseType(typeof(SuccessDataResult<List<DistrictDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Tüm ilçeleri listeler")]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await districtService.GetAllAsync(ct);
        return Ok(new SuccessDataResult<List<DistrictDto>>(result));
    }
}
