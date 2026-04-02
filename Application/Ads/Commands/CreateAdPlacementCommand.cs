using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class CreateAdPlacementCommand : IRequest<AdPlacementDTO>, ITransactionalRequest
{
    public Guid AdSlotId { get; set; }
    public Guid AdCreativeId { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? DailyCap { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAdPlacementCommandHandler(IRepository<AdPlacement> repo, IMapper mapper) : IRequestHandler<CreateAdPlacementCommand, AdPlacementDTO>
{
    public async Task<AdPlacementDTO> Handle(CreateAdPlacementCommand request, CancellationToken ct)
    {
        var placement = mapper.Map<AdPlacement>(request);
        await repo.AddAsync(placement, ct);
        return mapper.Map<AdPlacementDTO>(placement);
    }
}
