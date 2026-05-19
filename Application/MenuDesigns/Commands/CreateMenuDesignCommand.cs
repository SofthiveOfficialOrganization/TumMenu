using Application.Abstractions;
using Application.MenuDesigns;
using Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.MenuDesigns.Commands;

public sealed class CreateMenuDesignCommand : IRequest<MenuDesign>, ITransactionalRequest, IAuditableCommand, IMenuDesignForm
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    [Required] public string PrimaryColor { get; set; } = null!;
    [Required] public string PrimaryDarkColor { get; set; } = null!;
    [Required] public string AccentColor { get; set; } = null!;
    [Required] public string BackgroundColor { get; set; } = null!;
    [Required] public string SurfaceColor { get; set; } = null!;
    [Required] public string TextColor { get; set; } = null!;
    [Required] public string MutedColor { get; set; } = null!;
    [Required] public string BorderRadius { get; set; } = "24px";
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

    public string ActionName => "Menü tasarımı oluşturuldu";
}

public class CreateMenuDesignHandler(
    IRepository<MenuDesign> repoDesign
) : IRequestHandler<CreateMenuDesignCommand, MenuDesign>
{
    public async Task<MenuDesign> Handle(CreateMenuDesignCommand req, CancellationToken ct)
    {
        var design = new MenuDesign
        {
            Name = req.Name,
            Slug = req.Slug,
            Description = req.Description,
            PrimaryColor = req.PrimaryColor,
            PrimaryDarkColor = req.PrimaryDarkColor,
            AccentColor = req.AccentColor,
            BackgroundColor = req.BackgroundColor,
            SurfaceColor = req.SurfaceColor,
            TextColor = req.TextColor,
            MutedColor = req.MutedColor,
            BorderRadius = req.BorderRadius,
            BackgroundGradient = req.BackgroundGradient,
            BackgroundImageUrl = req.BackgroundImageUrl,
            PreviewImageUrl = req.PreviewImageUrl,
            HeaderBackgroundColor = req.HeaderBackgroundColor,
            HeaderTextColor = req.HeaderTextColor,
            ButtonStyle = req.ButtonStyle,
            CardShadow = req.CardShadow,
            CardBorderColor = req.CardBorderColor,
            CardBorderWidth = req.CardBorderWidth,
            CardBackgroundColor = req.CardBackgroundColor,
            CardImageBorderRadius = req.CardImageBorderRadius,
            FooterBackgroundColor = req.FooterBackgroundColor,
            FooterTextColor = req.FooterTextColor,
            FooterButtonColor = req.FooterButtonColor,
            FooterButtonStyle = req.FooterButtonStyle,
            FontFamily = req.FontFamily,
            BodyFontSize = req.BodyFontSize,
            HeadingFontFamily = req.HeadingFontFamily,
            HeadingFontSize = req.HeadingFontSize,
            SubheadingFontSize = req.SubheadingFontSize,
            HeadingTextColor = req.HeadingTextColor,
            HeadingBorderWidth = req.HeadingBorderWidth,
            HeadingBorderColor = req.HeadingBorderColor,
            HeadingFontWeight = req.HeadingFontWeight,
            LinkColor = req.LinkColor,
            PriceColor = req.PriceColor,
            ButtonTextColor = req.ButtonTextColor,
            ButtonBorderColor = req.ButtonBorderColor,
            ButtonBorderRadius = req.ButtonBorderRadius,
            SurfaceOpacity = req.SurfaceOpacity,
            CardOpacity = req.CardOpacity,
            HeaderSecondaryTextOpacity = req.HeaderSecondaryTextOpacity,
            HeaderAccentTextOpacity = req.HeaderAccentTextOpacity,
            PageBackgroundOverlayOpacity = req.PageBackgroundOverlayOpacity,
            MutedTextOpacity = req.MutedTextOpacity,
            PanelOpacity = req.PanelOpacity,
            HeaderChipOpacity = req.HeaderChipOpacity,
            IsDefault = req.IsDefault,
            SortOrder = req.SortOrder
        };
        await repoDesign.AddAsync(design, ct);
        return design;
    }
}
