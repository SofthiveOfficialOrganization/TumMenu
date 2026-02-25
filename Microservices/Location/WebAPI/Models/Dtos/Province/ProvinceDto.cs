using WebAPI.Models.Dtos.District;

namespace WebAPI.Models.Dtos.Province;

public record ProvinceDto(
    Guid Id, 
    string Name, 
    double Latitude, 
    double Longitude, 
    string GoogleMaps, 
    string OpenStreetMap,
    List<DistrictDto> Districts) : IDto;