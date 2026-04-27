using Domain.Entities;

namespace Application.MenuDesigns.DTOs;

public static class MenuDesignMappingExtensions
{
    public static MenuDesignDTO ToDto(this MenuDesign design) => new()
    {
        Id = design.Id,
        Name = design.Name,
        Slug = design.Slug,
        Description = design.Description,
        PrimaryColor = design.PrimaryColor,
        PrimaryDarkColor = design.PrimaryDarkColor,
        AccentColor = design.AccentColor,
        BackgroundColor = design.BackgroundColor,
        SurfaceColor = design.SurfaceColor,
        TextColor = design.TextColor,
        MutedColor = design.MutedColor,
        BorderRadius = design.BorderRadius,
        BackgroundGradient = design.BackgroundGradient,
        BackgroundImageUrl = design.BackgroundImageUrl,
        PreviewImageUrl = design.PreviewImageUrl,
        HeaderBackgroundColor = design.HeaderBackgroundColor,
        HeaderTextColor = design.HeaderTextColor,
        ButtonStyle = design.ButtonStyle,
        CardShadow = design.CardShadow,
        CardBorderColor = design.CardBorderColor,
        CardImageBorderRadius = design.CardImageBorderRadius,
        FooterBackgroundColor = design.FooterBackgroundColor,
        FooterTextColor = design.FooterTextColor,
        FooterButtonColor = design.FooterButtonColor,
        FooterButtonStyle = design.FooterButtonStyle,
        FontFamily = design.FontFamily,
        HeadingFontFamily = design.HeadingFontFamily,
        HeadingFontSize = design.HeadingFontSize,
        SubheadingFontSize = design.SubheadingFontSize,
        IsDefault = design.IsDefault,
        SortOrder = design.SortOrder
    };
}
