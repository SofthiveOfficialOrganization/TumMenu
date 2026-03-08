using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

#region AdSlot
public record CreateAdSlotCommand(string Key, string? Description, bool IsActive) : IRequest<AdSlotDTO>, ITransactionalRequest;

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

public record UpdateAdSlotCommand(Guid Id, string Key, string? Description, bool IsActive) : IRequest<AdSlotDTO>, ITransactionalRequest;

public class UpdateAdSlotCommandHandler(IRepository<AdSlot> repo, IMapper mapper) : IRequestHandler<UpdateAdSlotCommand, AdSlotDTO>
{
    public async Task<AdSlotDTO> Handle(UpdateAdSlotCommand request, CancellationToken ct)
    {
        var slot = await repo.GetByIdAsync(request.Id, ct) ?? throw new Exception("Slot not found");
        mapper.Map(request, slot);
        slot.Description ??= "";
        repo.Update(slot);
        return mapper.Map<AdSlotDTO>(slot);
    }
}
#endregion

#region AdCreative
public record CreateAdCreativeCommand(string Type, string Content, string? ClickUrl) : IRequest<AdCreativeDTO>, ITransactionalRequest;

public class CreateAdCreativeCommandHandler(IRepository<AdCreative> repo, IMapper mapper) : IRequestHandler<CreateAdCreativeCommand, AdCreativeDTO>
{
    public async Task<AdCreativeDTO> Handle(CreateAdCreativeCommand request, CancellationToken ct)
    {
        var creative = mapper.Map<AdCreative>(request);
        await repo.AddAsync(creative, ct);
        return mapper.Map<AdCreativeDTO>(creative);
    }
}

public record UpdateAdCreativeCommand(Guid Id, string Type, string Content, string? ClickUrl) : IRequest<AdCreativeDTO>, ITransactionalRequest;

public class UpdateAdCreativeCommandHandler(IRepository<AdCreative> repo, IMapper mapper) : IRequestHandler<UpdateAdCreativeCommand, AdCreativeDTO>
{
    public async Task<AdCreativeDTO> Handle(UpdateAdCreativeCommand request, CancellationToken ct)
    {
        var creative = await repo.GetByIdAsync(request.Id, ct) ?? throw new Exception("Creative not found");
        mapper.Map(request, creative);
        repo.Update(creative);
        return mapper.Map<AdCreativeDTO>(creative);
    }
}
#endregion

#region AdPlacement
public record CreateAdPlacementCommand(Guid AdSlotId, Guid AdCreativeId, DateTime? StartAt, DateTime? EndAt, int? DailyCap, bool IsActive) : IRequest<AdPlacementDTO>, ITransactionalRequest;

public class CreateAdPlacementCommandHandler(IRepository<AdPlacement> repo, IMapper mapper) : IRequestHandler<CreateAdPlacementCommand, AdPlacementDTO>
{
    public async Task<AdPlacementDTO> Handle(CreateAdPlacementCommand request, CancellationToken ct)
    {
        var placement = mapper.Map<AdPlacement>(request);
        await repo.AddAsync(placement, ct);
        return mapper.Map<AdPlacementDTO>(placement);
    }
}

public record UpdateAdPlacementCommand(Guid Id, Guid AdSlotId, Guid AdCreativeId, DateTime? StartAt, DateTime? EndAt, int? DailyCap, bool IsActive) : IRequest<AdPlacementDTO>, ITransactionalRequest;

public class UpdateAdPlacementCommandHandler(IRepository<AdPlacement> repo, IMapper mapper) : IRequestHandler<UpdateAdPlacementCommand, AdPlacementDTO>
{
    public async Task<AdPlacementDTO> Handle(UpdateAdPlacementCommand request, CancellationToken ct)
    {
        var placement = await repo.GetByIdAsync(request.Id, ct) ?? throw new Exception("Placement not found");
        mapper.Map(request, placement);
        repo.Update(placement);
        return mapper.Map<AdPlacementDTO>(placement);
    }
}
#endregion

#region Delete Commands
public record DeleteAdSlotCommand(Guid Id) : IRequest<bool>, ITransactionalRequest;
public class DeleteAdSlotCommandHandler(IRepository<AdSlot> repo) : IRequestHandler<DeleteAdSlotCommand, bool>
{
    public async Task<bool> Handle(DeleteAdSlotCommand request, CancellationToken ct)
    {
        var slot = await repo.GetByIdAsync(request.Id, ct);
        if (slot == null) return false;
        repo.SoftDelete(slot);
        return true;
    }
}

public record DeleteAdCreativeCommand(Guid Id) : IRequest<bool>, ITransactionalRequest;
public class DeleteAdCreativeCommandHandler(IRepository<AdCreative> repo) : IRequestHandler<DeleteAdCreativeCommand, bool>
{
    public async Task<bool> Handle(DeleteAdCreativeCommand request, CancellationToken ct)
    {
        var creative = await repo.GetByIdAsync(request.Id, ct);
        if (creative == null) return false;
        repo.SoftDelete(creative);
        return true;
    }
}

public record DeleteAdPlacementCommand(Guid Id) : IRequest<bool>, ITransactionalRequest;
public class DeleteAdPlacementCommandHandler(IRepository<AdPlacement> repo) : IRequestHandler<DeleteAdPlacementCommand, bool>
{
    public async Task<bool> Handle(DeleteAdPlacementCommand request, CancellationToken ct)
    {
        var placement = await repo.GetByIdAsync(request.Id, ct);
        if (placement == null) return false;
        repo.SoftDelete(placement);
        return true;
    }
}
#endregion
