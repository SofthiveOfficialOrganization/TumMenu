namespace WebAPI.Models.Dtos.District;

public record DistrictDto(
    Guid Id, 
    Guid ProvinceId, 
    string Name, 
    int Population, 
    int Area);
