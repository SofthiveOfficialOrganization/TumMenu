using Application.Abstractions;
using Application.Addresses.DTOs;
using Application.Common.Exceptions;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Stores.Commands;

public class CreateStoreCommand : IRequest<StoreDTO>, ITransactionalRequest
{
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public Guid CompanyId { get; set; }
	public AddressDTO? Address { get; set; }
}

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