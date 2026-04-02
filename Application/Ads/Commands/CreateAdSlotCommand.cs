using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class CreateAdSlotCommand : IRequest<AdSlotDTO>, ITransactionalRequest
{
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAdSlotCommandHandler(IRepository<AdSlot> repo, IMapper mapper) : IRequestHandler<CreateAdSlotCommand, AdSlotDTO>
{
    public async Task<AdSlotDTO> Handle(CreateAdSlotCommand request, CancellationToken ct)
    {
        var slot = mapper.Map<AdSlot>(request);
        slot.Description ??= "";
        await repo.AddAsync(slot, ct);
        return mapper.Map<AdSlotDTO>(slot);
    }
}
