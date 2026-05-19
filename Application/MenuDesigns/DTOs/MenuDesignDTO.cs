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
    public string? BackgroundImageUrl { get; set; }
    public string? PreviewImageUrl { get; set; }
    public string? HeaderBackgroundColor { get; set; }
    public string? HeaderTextColor { get; set; }
    public string ButtonStyle { get; set; } = "solid";
    public string? CardShadow { get; set; }
    public string? CardBorderColor { get; set; }
    public string? CardBorderWidth { get; set; }
    public string? CardBackgroundColor { get; set; }
    public string? CardImageBorderRadius { get; set; }
    public string? FooterBackgroundColor { get; set; }
    public string? FooterTextColor { get; set; }
    public string? FooterButtonColor { get; set; }
    public string? FooterButtonStyle { get; set; }
    public string? FontFamily { get; set; }
    public string? BodyFontSize { get; set; }
    public string? HeadingFontFamily { get; set; }
    public string? HeadingFontSize { get; set; }
    public string? SubheadingFontSize { get; set; }
    public string? HeadingTextColor { get; set; }
    public string? HeadingBorderWidth { get; set; }
    public string? HeadingBorderColor { get; set; }
    public string? HeadingFontWeight { get; set; }
    public string? LinkColor { get; set; }
    public string? PriceColor { get; set; }
    public string? ButtonTextColor { get; set; }
    public string? ButtonBorderColor { get; set; }
    public string? ButtonBorderRadius { get; set; }
    public string? SurfaceOpacity { get; set; }
    public string? CardOpacity { get; set; }
    public string? HeaderSecondaryTextOpacity { get; set; }
    public string? HeaderAccentTextOpacity { get; set; }
    public string? PageBackgroundOverlayOpacity { get; set; }
    public string? MutedTextOpacity { get; set; }
    public string? PanelOpacity { get; set; }
    public string? HeaderChipOpacity { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}
