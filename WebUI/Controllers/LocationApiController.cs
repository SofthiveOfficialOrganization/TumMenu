using Application.Microservices.Location;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    [ApiController]
    [Route("api/location")]
    public class LocationApiController(ILocationMicroservice locationMicroservice) : ControllerBase
    {
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await locationMicroservice.GetAllProvincesAsync();
            return Ok(provinces);
        }

        [HttpGet("provinces/{provinceId}")]
        public async Task<IActionResult> GetProvinceById(Guid provinceId)
        {
            var province = await locationMicroservice.GetProvinceBasicByIdAsync(provinceId);
            if (province == null) return NotFound();
            return Ok(province);
        }

        [HttpGet("districts/{provinceId}")]
        public async Task<IActionResult> GetDistrictsByProvinceId(Guid provinceId)
        {
            var districts = await locationMicroservice.GetDistrictsByProvinceIdAsync(provinceId);
            return Ok(districts);
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await locationMicroservice.GetAllCountriesAsync();
            return Ok(countries);
        }
    }
}
