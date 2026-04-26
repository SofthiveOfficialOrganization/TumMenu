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
    public string? PreviewImageUrl { get; set; }
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
        design.PreviewImageUrl = req.PreviewImageUrl;
        design.IsDefault = req.IsDefault;
        design.SortOrder = req.SortOrder;

        repoDesign.Update(design);
        return design;
    }
}
