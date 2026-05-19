namespace WebUI.Seo;

public sealed class RestaurantJsonLdModel
{
    public required string Name { get; init; }
    public required string PageUrl { get; init; }
    public string? MenuUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Telephone { get; init; }
    public string? StreetAddress { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}
