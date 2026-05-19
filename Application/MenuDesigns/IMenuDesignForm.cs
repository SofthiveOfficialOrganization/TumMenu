namespace Application.MenuDesigns;

/// <summary>Shared editable fields for create/update menu design forms.</summary>
public interface IMenuDesignForm
{
    string Name { get; set; }
    string Slug { get; set; }
    string? Description { get; set; }
    string PrimaryColor { get; set; }
    string PrimaryDarkColor { get; set; }
    string AccentColor { get; set; }
    string BackgroundColor { get; set; }
    string SurfaceColor { get; set; }
    string TextColor { get; set; }
    string MutedColor { get; set; }
    string BorderRadius { get; set; }
    string? BackgroundGradient { get; set; }
    string? BackgroundImageUrl { get; set; }
    string? PreviewImageUrl { get; set; }
    string? HeaderBackgroundColor { get; set; }
    string? HeaderTextColor { get; set; }
    string ButtonStyle { get; set; }
    string? CardShadow { get; set; }
    string? CardBorderColor { get; set; }
    string? CardBorderWidth { get; set; }
    string? CardBackgroundColor { get; set; }
    string? CardImageBorderRadius { get; set; }
    string? FooterBackgroundColor { get; set; }
    string? FooterTextColor { get; set; }
    string? FooterButtonColor { get; set; }
    string? FooterButtonStyle { get; set; }
    string? FontFamily { get; set; }
    string? BodyFontSize { get; set; }
    string? HeadingFontFamily { get; set; }
    string? HeadingFontSize { get; set; }
    string? SubheadingFontSize { get; set; }
    string? HeadingTextColor { get; set; }
    string? HeadingBorderWidth { get; set; }
    string? HeadingBorderColor { get; set; }
    string? HeadingFontWeight { get; set; }
    string? LinkColor { get; set; }
    string? PriceColor { get; set; }
    string? ButtonTextColor { get; set; }
    string? ButtonBorderColor { get; set; }
    string? ButtonBorderRadius { get; set; }
    string? SurfaceOpacity { get; set; }
    string? CardOpacity { get; set; }
    string? HeaderSecondaryTextOpacity { get; set; }
    string? HeaderAccentTextOpacity { get; set; }
    string? PageBackgroundOverlayOpacity { get; set; }
    string? MutedTextOpacity { get; set; }
    string? PanelOpacity { get; set; }
    string? HeaderChipOpacity { get; set; }
    bool IsDefault { get; set; }
    int SortOrder { get; set; }
}
