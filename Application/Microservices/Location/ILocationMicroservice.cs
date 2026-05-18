using Application.Microservices.Location.DTOs;

namespace Application.Microservices.Location
{
    public interface ILocationMicroservice
    {
        Task<List<GetCountriesResponseDTO>> GetAllCountriesAsync();
        Task<List<GetProvincesResponseDTO>> GetAllProvincesAsync();
        Task<List<GetDisctrictsResponseDTO>> GetDistrictsByProvinceIdAsync(Guid provinceId);
        Task<GetProvinceResponseBasicDTO?> GetProvinceBasicByIdAsync(Guid provinceId);
        Task<List<GeocodingResultDTO>> SearchGeocodingAsync(string query, int limit = 1);
    }
}
