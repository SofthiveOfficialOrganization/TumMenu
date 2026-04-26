namespace Application.MenuDesigns.DTOs;

public sealed class MenuDesignDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string PrimaryColor { get; set; } = null!;
    public string PrimaryDarkColor { get; set; } = null!;
    public string AccentColor { get; set; } = null!;
    public string BackgroundColor { get; set; } = null!;
    public string SurfaceColor { get; set; } = null!;
    public string TextColor { get; set; } = null!;
    public string MutedColor { get; set; } = null!;
    public string BorderRadius { get; set; } = null!;
    public string? BackgroundGradient { get; set; }
    public string? PreviewImageUrl { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
