using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.Province;
using WebAPI.Services.ProvinceServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProvincesController(IProvinceService provinceService) : BaseMicroserviceController
{
    [ProducesResponseType(typeof(SuccessDataResult<ProvinceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "ID ile il bilgisi getirir")]
    [HttpGet("[action]/{provinceId}")]
    public async Task<IActionResult> GetByProvinceId([FromRoute] Guid provinceId, CancellationToken ct = default)
    {
        var result = await provinceService.GetByProvinceIdAsync(provinceId, ct);
        return Ok(new SuccessDataResult<ProvinceDto>(result));
    }

    [ProducesResponseType(typeof(SuccessDataResult<List<ProvinceDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Tüm illeri listeler (ilçeler hariç)")]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetOnlyProvinces(CancellationToken ct = default)
    {
        var result = await provinceService.GetOnlyProvincesAsync(ct);
        return Ok(new SuccessDataResult<List<ProvinceDto>>(result));
    }

    [ProducesResponseType(typeof(SuccessDataResult<List<ProvinceDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Tüm illeri ilçeleriyle birlikte listeler")]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await provinceService.GetAllAsync(ct);
        return Ok(new SuccessDataResult<List<ProvinceDto>>(result));
    }

    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Türkiye API'den il/ilçe verilerini çekip LocalDB'e ekler")]
    [HttpPost("[action]")]
    public async Task<IActionResult> FetchTurkeyData(CancellationToken ct = default)
    {
        await provinceService.FetchTurkeyData(ct);
        return Ok(new SuccessResult(message: "İl ve ilçe bilgileri veri tabanına eklendi."));
    }
}
