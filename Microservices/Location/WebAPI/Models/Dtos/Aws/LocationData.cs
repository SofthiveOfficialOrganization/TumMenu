namespace WebAPI.Models.Dtos.Aws;

public class LocationData : IDto
{
    public string? Label { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Country { get; set; }
    public string? Municipality { get; set; }
    public string? SubMunicipality { get; set; }
    public string? Region { get; set; }
    public string? SubRegion { get; set; }
}