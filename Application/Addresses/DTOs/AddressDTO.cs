namespace Application.Addresses.DTOs;

public class AddressDTO
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? CountryId { get; set; }
    public string? CountryName { get; set; }
    public Guid? CityId { get; set; }
    public string? CityName { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? Neighborhood { get; set; }
    public string? FullAddress { get; set; }
}
