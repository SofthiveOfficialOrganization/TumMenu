using Application.Abstractions;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Stores.Commands;

public sealed record CreateStoreCommand
(
    string Name,
    string Slug,
    string PhoneNumber,
    Guid CompanyId
) : IRequest<StoreDTO>, ITransactionalRequest;

public class CreateStoreCommandHandler(
    IRepository<Store> repoStore,
    IMapper mapper
) : IRequestHandler<CreateStoreCommand, StoreDTO>
{
    public async Task<StoreDTO> Handle(CreateStoreCommand req, CancellationToken ct)
    {
        var store = mapper.Map<Store>(req);
        await repoStore.AddAsync(store, ct);
        var storeDTO = mapper.Map<StoreDTO>(store);
        return storeDTO;
    }
}