namespace WebAPI.Models.Dtos.TurkeyApi;

public class CityDataDto
{
    public string? Name { get; set; }
    public int Population { get; set; }
    public int Area { get; set; }
    public CoordinatesData? Coordinates { get; set; }
    public MapsData? Maps { get; set; }
    public List<DistrictData>? Districts { get; set; }
}
