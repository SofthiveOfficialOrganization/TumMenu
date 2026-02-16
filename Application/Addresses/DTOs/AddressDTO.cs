namespace Application.Addresses.DTOs;

public class AddressDTO
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? DistrictId { get; set; }
    public string? Neighborhood { get; set; }
    public string? FullAddress { get; set; }
}
