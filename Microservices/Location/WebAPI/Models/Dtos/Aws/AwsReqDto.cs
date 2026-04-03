namespace WebAPI.Models.Dtos.Aws;

public class AwsReqDto : IDto
{
    public string? Address { get; set; }
}
public class AwsResDto : IDto
{
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}