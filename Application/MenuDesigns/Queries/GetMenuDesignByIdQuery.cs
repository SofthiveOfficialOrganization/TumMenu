using Application.Abstractions;
using Application.MenuDesigns.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuDesigns.Queries;

public sealed record GetMenuDesignByIdQuery(Guid Id) : IRequest<MenuDesignDTO>;

public class GetMenuDesignByIdHandler(
    IRepository<MenuDesign> repoDesign
) : IRequestHandler<GetMenuDesignByIdQuery, MenuDesignDTO>
{
    public async Task<MenuDesignDTO> Handle(GetMenuDesignByIdQuery req, CancellationToken ct)
    {
        var d = await repoDesign.Query()
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct)
            ?? throw new KeyNotFoundException("Tasarım bulunamadı.");

        return new MenuDesignDTO
        {
            Id = d.Id,
            Name = d.Name,
            Slug = d.Slug,
            Description = d.Description,
            PrimaryColor = d.PrimaryColor,
            PrimaryDarkColor = d.PrimaryDarkColor,
            AccentColor = d.AccentColor,
            BackgroundColor = d.BackgroundColor,
            SurfaceColor = d.SurfaceColor,
            TextColor = d.TextColor,
            MutedColor = d.MutedColor,
            BorderRadius = d.BorderRadius,
            BackgroundGradient = d.BackgroundGradient,
            PreviewImageUrl = d.PreviewImageUrl,
            IsDefault = d.IsDefault,
            SortOrder = d.SortOrder
        };
    }
}
