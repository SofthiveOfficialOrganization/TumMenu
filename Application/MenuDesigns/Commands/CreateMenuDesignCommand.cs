using Application.Abstractions;
using Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.MenuDesigns.Commands;

public sealed class CreateMenuDesignCommand : IRequest<MenuDesign>, ITransactionalRequest, IAuditableCommand
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
    public string? CardImageBorderRadius { get; set; }
    public string? FooterBackgroundColor { get; set; }
    public string? FooterTextColor { get; set; }
    public string? FooterButtonColor { get; set; }
    public string? FooterButtonStyle { get; set; }
    public string? FontFamily { get; set; }
    public string? HeadingFontFamily { get; set; }
    public string? HeadingFontSize { get; set; }
    public string? SubheadingFontSize { get; set; }
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
            CardImageBorderRadius = req.CardImageBorderRadius,
            FooterBackgroundColor = req.FooterBackgroundColor,
            FooterTextColor = req.FooterTextColor,
            FooterButtonColor = req.FooterButtonColor,
            FooterButtonStyle = req.FooterButtonStyle,
            FontFamily = req.FontFamily,
            HeadingFontFamily = req.HeadingFontFamily,
            HeadingFontSize = req.HeadingFontSize,
            SubheadingFontSize = req.SubheadingFontSize,
            IsDefault = req.IsDefault,
            SortOrder = req.SortOrder
        };
        await repoDesign.AddAsync(design, ct);
        return design;
    }
}
