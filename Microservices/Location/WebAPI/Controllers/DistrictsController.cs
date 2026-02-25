using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Constants;
using WebAPI.DataAccess.Dynamic;
using WebAPI.DataAccess.Paging;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.District;
using WebAPI.Services.DistrictServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DistrictsController(
    IDistrictService districtService
    ) : BaseMicroserviceController
{
    private readonly IDistrictService _districtService = districtService;
    
    [ProducesResponseType(typeof(SuccessDataResult<List<DistrictDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.DISTRICTS_GET_LIST_BY_PROVINCE_ID)]
    [HttpGet("[action]/{provinceId}")]
    public async Task<IActionResult> GetListByProvinceId([FromRoute] Guid provinceId)
    {
        var result = await _districtService.GetListByProvinceIdAsync(provinceId);
        return Ok(new SuccessDataResult<List<DistrictDto>>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<List<DistrictDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.DISTRICTS_GET_ALL)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _districtService.GetAllAsync();
        return Ok(new SuccessDataResult<List<DistrictDto>>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<DistrictListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.DISTRICTS_GET_LIST)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        var result = await _districtService.GetListAsync(pageRequest);
        return Ok(new SuccessDataResult<DistrictListModel>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<DistrictListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.DISTRICTS_GET_LIST_BY_DYNAMIC)]
    [HttpPost("[action]")]
    public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery dynamicQuery)
    {
        var result = await _districtService.GetListByDynamicAsync(pageRequest, dynamicQuery);
        return Ok(new SuccessDataResult<DistrictListModel>(result));
    }
}