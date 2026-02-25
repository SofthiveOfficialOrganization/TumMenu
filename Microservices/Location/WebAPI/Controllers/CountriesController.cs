using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Constants;
using WebAPI.DataAccess.Dynamic;
using WebAPI.DataAccess.Paging;
using WebAPI.Exceptions.HttpProblemDetails;
using WebAPI.Models.Dtos.Country;
using WebAPI.Services.CountryServices;
using WebAPI.Utils.Results.Concrete;
using ValidationProblemDetails = Microsoft.AspNetCore.Mvc.ValidationProblemDetails;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(ICountryService countryService) : BaseMicroserviceController
{
    private readonly ICountryService _countryService = countryService;

    // [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    // [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    // [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    // [SwaggerOperation(description: ResponseDescriptions.COUNTRIES_FETCH_DATA)]
    // [HttpPost("[action]")]
    // public async Task<IActionResult> FetchCountries()
    // {
    //     await _countryService.FetchCountriesData();
    //     return Ok(new SuccessResult(message: "Ülkeler başarıyla veri tabanına eklendi."));
    // }
    //
    // [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    // [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    // [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    // [SwaggerOperation(description: ResponseDescriptions.COUNTRIES_CLEAR_DATA)]
    // [HttpDelete("[action]")]
    // public async Task<IActionResult> ClearCountries()
    // {
    //     await _countryService.ClearCountries();
    //     return Ok(new SuccessResult(message: "Tüm ülkeler veri tabanından silindi."));
    // }

    [ProducesResponseType(typeof(SuccessDataResult<List<CountryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.COUNTRIES_GET_ALL)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _countryService.GetAllCountriesAsync();
        return Ok(new SuccessDataResult<List<CountryDto>>(result));
    }
    
    /// <summary>
    /// Tüm ülkelerin temel bilgilerini getirir.
    /// </summary>
    [ProducesResponseType(typeof(SuccessDataResult<List<CountryBasicInfoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.COUNTRIES_GET_ALL_BASIC_INFO)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllBasicInfo()
    {
        var result = await _countryService.GetAllCountriesBasicInfoAsync();
        return Ok(new SuccessDataResult<List<CountryBasicInfoDto>>(result));
    }

    /// <summary>
    /// Sayfalama desteğiyle ülke bilgilerini listeler.
    /// </summary>
    [ProducesResponseType(typeof(SuccessDataResult<CountryListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.COUNTRIES_GET_LIST)]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        var result = await _countryService.GetListAsync(pageRequest);
        return Ok(new SuccessDataResult<CountryListModel>(result));
    }
    
    [ProducesResponseType(typeof(SuccessDataResult<CountryListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AuthorizationProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDataResult<BusinessProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDataResult<NotFoundProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDataResult<ValidationProblemDetails>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorDataResult<InternalServerErrorProblemDetails>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(description: ResponseDescriptions.COUNTRIES_GET_LIST_BY_DYNAMIC)]
    [HttpPost("[action]")]
    public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery dynamicQuery)
    {
        var result = await _countryService.GetListByDynamicAsync(pageRequest, dynamicQuery);
        return Ok(new SuccessDataResult<CountryListModel>(result));
    }
}