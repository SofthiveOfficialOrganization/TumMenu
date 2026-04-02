using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class UpdateAdPlacementCommand : IRequest<AdPlacementDTO>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid AdSlotId { get; set; }
    public Guid AdCreativeId { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? DailyCap { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateAdPlacementCommandHandler(IRepository<AdPlacement> repo, IMapper mapper) : IRequestHandler<UpdateAdPlacementCommand, AdPlacementDTO>
{
    public async Task<AdPlacementDTO> Handle(UpdateAdPlacementCommand request, CancellationToken ct)
    {
        var placement = await repo.GetByIdAsync(request.Id, ct) ?? throw new Exception("Yerleştirme bulunamadı");
        mapper.Map(request, placement);
        repo.Update(placement);
        return mapper.Map<AdPlacementDTO>(placement);
    }
}
