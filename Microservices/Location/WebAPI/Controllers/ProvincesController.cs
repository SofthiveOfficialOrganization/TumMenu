using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Constants;
using WebAPI.DataAccess.Dynamic;
using WebAPI.DataAccess.Paging;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.Province;
using WebAPI.Models.Dtos.TurkeyApi;
using WebAPI.Services.ProvinceServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProvincesController(
    IProvinceService provinceService
    ) : BaseMicroserviceController
{
    private readonly IProvinceService _provinceService = provinceService;

    // [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    // [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    // [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    // [SwaggerOperation(description: ResponseDescriptions.PROVINCES_FETCH_TURKEY_DATA)]
    // [HttpPost("[action]")]
    // public async Task<IActionResult> FetchTurkeyData()
    // {
    //     await _provinceService.FetchTurkeyData();
    //     return Ok(new SuccessResult(message: "İl ve ilçe bilgileri veri tabanına eklendi."));
    // }
    //
    // [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    // [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    // [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    // [SwaggerOperation(description: ResponseDescriptions.PROVINCES_CLEAR_DISTRICT_AND_PROVINCES)]
    // [HttpDelete("[action]")]
    // public async Task<IActionResult> ClearDistrictAndProvinces()
    // {
    //     await _provinceService.ClearDistrictAndProvinces();
    //     return Ok(new SuccessResult(message: "İl ve ilçe bilgileri veri tabanından silindi."));
    // }
    
    [ProducesResponseType(typeof(SuccessDataResult<ProvinceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.PROVINCES_CLEAR_DISTRICT_AND_PROVINCES)]
    [HttpGet("[action]/{provinceId}")]
    public async Task<IActionResult> GetByProvinceId([FromRoute] Guid provinceId)
    {
        var result = await _provinceService.GetByProvinceIdAsync(provinceId);
        return Ok(new SuccessDataResult<ProvinceDto>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<List<ProvinceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.PROVINCES_GET_ALL)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _provinceService.GetAllAsync();
        return Ok(new SuccessDataResult<List<ProvinceDto>>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<List<ProvinceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.PROVINCES_GET_ONLY_PROVINCES)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetOnlyProvinces()
    {
        var result = await _provinceService.GetOnlyProvincesAsync();
        return Ok(new SuccessDataResult<List<ProvinceDto>>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<ProvinceListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.PROVINCES_GET_LIS)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        var result = await _provinceService.GetListAsync(pageRequest);
        return Ok(new SuccessDataResult<ProvinceListModel>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<ProvinceListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.PROVINCES_GET_LIST_BY_DYNAMIC)]
    [HttpPost("[action]")]
    public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery dynamicQuery)
    {
        var result = await _provinceService.GetListByDynamicAsync(pageRequest, dynamicQuery);
        return Ok(new SuccessDataResult<ProvinceListModel>(result));
    }
}