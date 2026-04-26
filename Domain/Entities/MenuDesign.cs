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
    public string? CardImageBorderRadius { get; set; }

    // Footer
    public string? FooterBackgroundColor { get; set; }
    public string? FooterTextColor { get; set; }
    public string? FooterButtonColor { get; set; }
    public string? FooterButtonStyle { get; set; }

    // Typography (Yazı fontları & başlık büyüklükleri)
    public string? FontFamily { get; set; }
    public string? HeadingFontFamily { get; set; }
    public string? HeadingFontSize { get; set; }
    public string? SubheadingFontSize { get; set; }

    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Menu> Menus { get; set; } = [];
}
