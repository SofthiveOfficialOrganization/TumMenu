using Application.Abstractions;
using Application.Addresses.DTOs;
using Application.Common.Exceptions;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Commands;

public class CreateStoreCommand : IRequest<StoreDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Dükkan oluşturuldu";
	public string Title { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public Guid CompanyId { get; set; }
	public AddressDTO? Address { get; set; }
}

public class CreateStoreCommandHandler(
	IRepository<Store> repoStore,
	IMapper mapper,
	IRepository<Company> repoCompany,
    IMediator mediator
) : IRequestHandler<CreateStoreCommand, StoreDTO>
{
	public async Task<StoreDTO> Handle(CreateStoreCommand req, CancellationToken ct)
	{
		bool companyExists = await repoCompany.ExistsAsync(c => c.Id == req.CompanyId, ct);
		if(!companyExists)
			throw new UnprocessableAppException("Dükkanın ekleneceği şirket bulunamadı.");

		var slugExistsInCompany = await repoStore.Query()
			.AnyAsync(s => s.CompanyId == req.CompanyId && s.Slug == req.Slug, ct);
		if(slugExistsInCompany)
			throw new AlreadyExistsAppException("Bu slug zaten kullanılmakta.");

		var store = mapper.Map<Store>(req);
		await repoStore.AddAsync(store, ct);

        // Generate QR Code automatically
        await mediator.Send(new Application.QRs.Commands.GenerateQRCodeCommand { StoreId = store.Id }, ct);
        
        var storeDTO = mapper.Map<StoreDTO>(store);
		return storeDTO;
	}
}
