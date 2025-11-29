using Application.Abstractions;
using Application.Owners.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Owners.Commands;

public sealed record CreateOwnerCommand
(
    string ApplicationUserId
) : IRequest<OwnerDTO>, ITransactionalRequest;

public class CreateOwnerCommandHandler(
    IRepository<Owner> repoOwner,
    IMapper mapper
) : IRequestHandler<CreateOwnerCommand, OwnerDTO>
{
    public async Task<OwnerDTO> Handle(CreateOwnerCommand req, CancellationToken ct)
    {
        var owner = mapper.Map<Owner>(req);
        await repoOwner.AddAsync(owner, ct);
        var ownerDTO = mapper.Map<OwnerDTO>(owner);
        return ownerDTO;
    }
}