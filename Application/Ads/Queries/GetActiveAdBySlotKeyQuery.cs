using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Queries;

public record GetActiveAdBySlotKeyQuery(string SlotKey) : IRequest<AdPlacementDTO?>;

public class GetActiveAdBySlotKeyQueryHandler(
    IRepository<AdPlacement> repoPlacement,
    IMapper mapper
) : IRequestHandler<GetActiveAdBySlotKeyQuery, AdPlacementDTO?>
{
    public async Task<AdPlacementDTO?> Handle(GetActiveAdBySlotKeyQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var placement = await repoPlacement.Query()
            .Include(p => p.AdSlot)
            .Include(p => p.AdCreative)
            .Where(p => p.AdSlot.Key == request.SlotKey)
            .Where(p => p.IsActive && p.AdSlot.IsActive)
            .Where(p => (p.StartAt == null || p.StartAt <= now) && (p.EndAt == null || p.EndAt >= now))
            .OrderBy(p => Guid.NewGuid()) // Basic randomization for now
            .FirstOrDefaultAsync(ct);

        return placement == null ? null : mapper.Map<AdPlacementDTO>(placement);
    }
}
