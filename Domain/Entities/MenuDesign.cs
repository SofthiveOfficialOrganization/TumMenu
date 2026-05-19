using Domain.Base;

namespace Domain.Entities;

public class MenuDesign : BaseEntity
{
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
    public string BorderRadius { get; set; } = "24px";
    public string? BackgroundGradient { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? PreviewImageUrl { get; set; }

    // Header (Menü üst kısım)
    public string? HeaderBackgroundColor { get; set; }
    public string? HeaderTextColor { get; set; }

    // Buttons (Tuş stili)
    public string ButtonStyle { get; set; } = "solid"; // solid | outline | ghost

    // Cards (İçerik kartları)
    public string? CardShadow { get; set; }
    public string? CardBorderColor { get; set; }
    public string? CardBorderWidth { get; set; }
    public string? CardBackgroundColor { get; set; }
    public string? CardImageBorderRadius { get; set; }

    // Footer
    public string? FooterBackgroundColor { get; set; }
    public string? FooterTextColor { get; set; }
    public string? FooterButtonColor { get; set; }
    public string? FooterButtonStyle { get; set; }

    // Typography (Yazı fontları & başlık büyüklükleri)
    public string? FontFamily { get; set; }
    public string? BodyFontSize { get; set; }
    public string? HeadingFontFamily { get; set; }
    public string? HeadingFontSize { get; set; }
    public string? SubheadingFontSize { get; set; }
    public string? HeadingTextColor { get; set; }
    public string? HeadingBorderWidth { get; set; }
    public string? HeadingBorderColor { get; set; }
    public string? HeadingFontWeight { get; set; }

    // Buttons & links
    public string? LinkColor { get; set; }
    public string? PriceColor { get; set; }
    public string? ButtonTextColor { get; set; }
    public string? ButtonBorderColor { get; set; }
    public string? ButtonBorderRadius { get; set; }

    // Opacity (0–100, örn. "86" veya "86%")
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
    public ICollection<Menu> Menus { get; set; } = [];
}
