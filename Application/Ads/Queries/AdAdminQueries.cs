using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Queries;

public record GetAdSlotsQuery() : IRequest<List<AdSlotDTO>>;

public class GetAdSlotsQueryHandler(IRepository<AdSlot> repo, IMapper mapper) 
    : IRequestHandler<GetAdSlotsQuery, List<AdSlotDTO>>
{
    public async Task<List<AdSlotDTO>> Handle(GetAdSlotsQuery request, CancellationToken ct)
    {
        var slots = await repo.Query().ToListAsync(ct);
        return mapper.Map<List<AdSlotDTO>>(slots);
    }
}

public record GetAdCreativesQuery() : IRequest<List<AdCreativeDTO>>;

public class GetAdCreativesQueryHandler(IRepository<AdCreative> repo, IMapper mapper) 
    : IRequestHandler<GetAdCreativesQuery, List<AdCreativeDTO>>
{
    public async Task<List<AdCreativeDTO>> Handle(GetAdCreativesQuery request, CancellationToken ct)
    {
        var creatives = await repo.Query().ToListAsync(ct);
        return mapper.Map<List<AdCreativeDTO>>(creatives);
    }
}

public record GetAdPlacementsQuery() : IRequest<List<AdPlacementDTO>>;

public class GetAdPlacementsQueryHandler(IRepository<AdPlacement> repo, IMapper mapper) 
    : IRequestHandler<GetAdPlacementsQuery, List<AdPlacementDTO>>
{
    public async Task<List<AdPlacementDTO>> Handle(GetAdPlacementsQuery request, CancellationToken ct)
    {
        var placements = await repo.Query()
            .Include(p => p.AdSlot)
            .Include(p => p.AdCreative)
            .ToListAsync(ct);
        return mapper.Map<List<AdPlacementDTO>>(placements);
    }
}
