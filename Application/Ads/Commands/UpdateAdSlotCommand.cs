using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class UpdateAdSlotCommand : IRequest<AdSlotDTO>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateAdSlotCommandHandler(IRepository<AdSlot> repo, IMapper mapper) : IRequestHandler<UpdateAdSlotCommand, AdSlotDTO>
{
    public async Task<AdSlotDTO> Handle(UpdateAdSlotCommand request, CancellationToken ct)
    {
        var slot = await repo.GetByIdAsync(request.Id, ct) ?? throw new Exception("Slot bulunamadı");
        mapper.Map(request, slot);
        slot.Description ??= "";
        repo.Update(slot);
        return mapper.Map<AdSlotDTO>(slot);
    }
}
