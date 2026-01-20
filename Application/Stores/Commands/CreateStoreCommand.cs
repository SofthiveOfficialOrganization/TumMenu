using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Stores.Commands;

public sealed record CreateStoreCommand
(
	string Title,
	string Slug,
	string PhoneNumber,
	Guid CompanyId
) : IRequest<StoreDTO>, ITransactionalRequest;

public class CreateStoreCommandHandler(
	IRepository<Store> repoStore,
	IMapper mapper,
	IRepository<Company> repoCompany
) : IRequestHandler<CreateStoreCommand, StoreDTO>
{
	public async Task<StoreDTO> Handle(CreateStoreCommand req, CancellationToken ct)
	{
		bool companyExists = await repoCompany.ExistsAsync(c => c.Id == req.CompanyId, ct);
		if(!companyExists)
			throw new UnprocessableAppException("Dükkanın ekleneceği şirket bulunamadı.");
		var store = mapper.Map<Store>(req);
		await repoStore.AddAsync(store, ct);
		var storeDTO = mapper.Map<StoreDTO>(store);
		return storeDTO;
	}
}