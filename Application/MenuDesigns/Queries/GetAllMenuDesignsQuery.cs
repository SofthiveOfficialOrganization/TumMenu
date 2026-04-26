using Application.Abstractions;
using Application.MenuDesigns.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.MenuDesigns.Queries;

public sealed record GetAllMenuDesignsQuery : IRequest<List<MenuDesignDTO>>;

public class GetAllMenuDesignsHandler(
    IRepository<MenuDesign> repoDesign
) : IRequestHandler<GetAllMenuDesignsQuery, List<MenuDesignDTO>>
{
    public async Task<List<MenuDesignDTO>> Handle(GetAllMenuDesignsQuery req, CancellationToken ct)
    {
        return await repoDesign.Query()
            .OrderBy(d => d.SortOrder)
            .Select(d => new MenuDesignDTO
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
                BackgroundImageUrl = d.BackgroundImageUrl,
                PreviewImageUrl = d.PreviewImageUrl,
                HeaderBackgroundColor = d.HeaderBackgroundColor,
                HeaderTextColor = d.HeaderTextColor,
                ButtonStyle = d.ButtonStyle,
                CardShadow = d.CardShadow,
                CardBorderColor = d.CardBorderColor,
                CardImageBorderRadius = d.CardImageBorderRadius,
                FooterBackgroundColor = d.FooterBackgroundColor,
                FooterTextColor = d.FooterTextColor,
                FooterButtonColor = d.FooterButtonColor,
                FooterButtonStyle = d.FooterButtonStyle,
                FontFamily = d.FontFamily,
                HeadingFontFamily = d.HeadingFontFamily,
                HeadingFontSize = d.HeadingFontSize,
                SubheadingFontSize = d.SubheadingFontSize,
                IsDefault = d.IsDefault,
                SortOrder = d.SortOrder
            })
            .ToListAsync(ct);
    }
}
