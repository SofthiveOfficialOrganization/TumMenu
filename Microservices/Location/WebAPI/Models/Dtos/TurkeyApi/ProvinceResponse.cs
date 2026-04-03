namespace WebAPI.Models.Dtos.TurkeyApi;

public class ProvinceResponse
{
    public string? Status { get; set; }
    public List<CityDataDto>? Data { get; set; }
}