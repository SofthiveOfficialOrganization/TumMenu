using Application.Abstractions;
using Application.Addresses.DTOs;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Commands;

public sealed record UpdateStoreCommand
(
	Guid Id,
	string Title,
	string Slug,
	string PhoneNumber,
	AddressDTO? Address
) : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
	public string ActionName => "Dükkan güncellendi";
	public Guid EntityId => Id;
}

public class UpdateStoreCommandHandler(
	IRepository<Store> repoStore,
	IMapper mapper
) : IRequestHandler<UpdateStoreCommand, Unit>
{
	public async Task<Unit> Handle(UpdateStoreCommand req, CancellationToken ct)
	{
		var store = await repoStore.Query(tracked: true)
					.Include(s => s.Address)
					.FirstOrDefaultAsync(s => s.Id == req.Id, ct) ?? throw new NotFoundAppException("Dükkan bulunamadı.");

		mapper.Map(req, store);
		repoStore.Update(store);
		return Unit.Value;
	}
}