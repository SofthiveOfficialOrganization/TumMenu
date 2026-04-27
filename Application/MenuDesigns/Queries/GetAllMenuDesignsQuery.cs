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
        var designs = await repoDesign.Query()
            .OrderBy(d => d.SortOrder)
            .ToListAsync(ct);

        return designs.Select(d => d.ToDto()).ToList();
    }
}
