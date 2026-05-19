namespace WebUI.Seo;

public sealed class MenuItemJsonLdModel
{
    public required string Name { get; init; }
    public required string PageUrl { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public bool IncludeOffer { get; init; }
}
