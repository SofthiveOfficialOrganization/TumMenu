using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed class GetStoresPaginatedByCurrentUserQuery : PageRequest, IRequest<PaginatedListDTO<StoreDTO>>
{
	public Guid? CompanyId { get; set; }
}

public class GetStoresPaginatedByCurrentUserHandler(
	IRepository<Store> repoStore,
	IUserContext userContext,
	IMapper mapper
) : IRequestHandler<GetStoresPaginatedByCurrentUserQuery, PaginatedListDTO<StoreDTO>>
{
	public async Task<PaginatedListDTO<StoreDTO>> Handle(GetStoresPaginatedByCurrentUserQuery req, CancellationToken ct)
	{
		var userId = userContext.UserId;
		var stores = await repoStore.GetPageListAsync(req,
			expression: s => 
				(req.CompanyId == null || s.CompanyId == req.CompanyId) &&
				(userContext.Roles.Contains("Admin") || s.Company.Owner!.ApplicationUserId == userId),
			include: s => s
				.Include(s => s.Menus)
				.Include(s => s.Company)
				.Include(s => s.Address),
			ct: ct);

		var storeDTO = mapper.Map<PaginatedListDTO<StoreDTO>>(stores);
		return storeDTO;
	}
}