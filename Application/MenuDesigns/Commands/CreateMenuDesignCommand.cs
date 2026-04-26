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
    public string? PreviewImageUrl { get; set; }
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
            PreviewImageUrl = req.PreviewImageUrl,
            IsDefault = req.IsDefault,
            SortOrder = req.SortOrder
        };
        await repoDesign.AddAsync(design, ct);
        return design;
    }
}
