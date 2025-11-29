using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed record GetStoresPaginatedByCompanyIdQuery
(
	Guid CompanyId
) : PageRequest, IRequest<PaginatedListDTO<StoreDTO>>;

public class GetStoresPaginatedByCompanyIdHandler(
	IRepository<Store> repoStore,
	IMapper mapper
) : IRequestHandler<GetStoresPaginatedByCompanyIdQuery, PaginatedListDTO<StoreDTO>>
{
	public async Task<PaginatedListDTO<StoreDTO>> Handle(GetStoresPaginatedByCompanyIdQuery req, CancellationToken ct)
	{
		var stores = await repoStore.GetPageListAsync(req,
			expression: s => s.CompanyId == req.CompanyId,
			include: s => s
				.Include(s => s.Menus)
				.Include(s => s.Company)
				.Include(s => s.Address),
			ct: ct);

		var storeDTO = mapper.Map<PaginatedListDTO<StoreDTO>>(stores);
		return storeDTO;
	}
}