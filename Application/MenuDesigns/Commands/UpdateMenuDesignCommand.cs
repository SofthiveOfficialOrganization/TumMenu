using Application.Abstractions;
using Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.MenuDesigns.Commands;

public sealed class UpdateMenuDesignCommand : IRequest<MenuDesign>, ITransactionalRequest, IEntityAuditableCommand
{
    public Guid Id { get; set; }
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

    public string ActionName => "Menü tasarımı güncellendi";
    public Guid EntityId => Id;
}

public class UpdateMenuDesignHandler(
    IRepository<MenuDesign> repoDesign
) : IRequestHandler<UpdateMenuDesignCommand, MenuDesign>
{
    public async Task<MenuDesign> Handle(UpdateMenuDesignCommand req, CancellationToken ct)
    {
        var design = await repoDesign.GetByIdAsync(req.Id, ct)
            ?? throw new KeyNotFoundException("Tasarım bulunamadı.");

        design.Name = req.Name;
        design.Slug = req.Slug;
        design.Description = req.Description;
        design.PrimaryColor = req.PrimaryColor;
        design.PrimaryDarkColor = req.PrimaryDarkColor;
        design.AccentColor = req.AccentColor;
        design.BackgroundColor = req.BackgroundColor;
        design.SurfaceColor = req.SurfaceColor;
        design.TextColor = req.TextColor;
        design.MutedColor = req.MutedColor;
        design.BorderRadius = req.BorderRadius;
        design.BackgroundGradient = req.BackgroundGradient;
        design.BackgroundImageUrl = req.BackgroundImageUrl;
        design.PreviewImageUrl = req.PreviewImageUrl;
        design.HeaderBackgroundColor = req.HeaderBackgroundColor;
        design.HeaderTextColor = req.HeaderTextColor;
        design.ButtonStyle = req.ButtonStyle;
        design.CardShadow = req.CardShadow;
        design.CardBorderColor = req.CardBorderColor;
        design.CardImageBorderRadius = req.CardImageBorderRadius;
        design.FooterBackgroundColor = req.FooterBackgroundColor;
        design.FooterTextColor = req.FooterTextColor;
        design.FooterButtonColor = req.FooterButtonColor;
        design.FooterButtonStyle = req.FooterButtonStyle;
        design.FontFamily = req.FontFamily;
        design.HeadingFontFamily = req.HeadingFontFamily;
        design.HeadingFontSize = req.HeadingFontSize;
        design.SubheadingFontSize = req.SubheadingFontSize;
        design.IsDefault = req.IsDefault;
        design.SortOrder = req.SortOrder;

        repoDesign.Update(design);
        return design;
    }
}
