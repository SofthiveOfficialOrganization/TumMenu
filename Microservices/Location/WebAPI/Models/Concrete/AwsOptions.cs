namespace WebAPI.Models.Concrete;

public class AwsOptions
{
    public required string Region { get; set; }
    public required string ApiKey { get; set; }
    public required string PlaceIndex { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
}