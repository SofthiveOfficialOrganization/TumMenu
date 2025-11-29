using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Stores.Commands;

public sealed record UpdateStoreCommand
(
	Guid Id,
	string Name,
	string Slug,
	string PhoneNumber
) : IRequest<Unit>, ITransactionalRequest;

public class UpdateStoreCommandHandler(
	IRepository<Store> repoStore,
	IMapper mapper
) : IRequestHandler<UpdateStoreCommand, Unit>
{
	public async Task<Unit> Handle(UpdateStoreCommand req, CancellationToken ct)
	{
		var store = await repoStore.GetByIdAsync(req.Id, ct) ?? throw new NotFoundAppException("Dükkan bulunamadı.");
		mapper.Map(req, store);
		repoStore.Update(store);
		return Unit.Value;
	}
}