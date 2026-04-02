using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public sealed class GetStoresPagedQuery : PageRequest, IRequest<PaginatedListDTO<StoreDTO>>, IAuthorizedRequest
{
	public Guid? CompanyId { get; set; }
	public string? Search { get; set; }
}

public class GetStoresPagedHandler(
	IRepository<Store> repoStore,
	IUserContext userContext,
	IMapper mapper
) : IRequestHandler<GetStoresPagedQuery, PaginatedListDTO<StoreDTO>>
{
	public async Task<PaginatedListDTO<StoreDTO>> Handle(GetStoresPagedQuery req, CancellationToken ct)
	{
		var userId = userContext.UserId;
		var stores = await repoStore.GetPageListAsync(req,
			expression: s => 
				(string.IsNullOrEmpty(req.Search) || s.Title.Contains(req.Search) || s.Slug.Contains(req.Search)) &&
				(req.CompanyId == null || s.CompanyId == req.CompanyId) &&
				(userContext.IsAdmin || s.Company.Owner!.ApplicationUserId == userId),
			include: s => s
				.Include(s => s.Menus)
				.Include(s => s.Company)
				.Include(s => s.Address),
			orderBy: q => q.OrderBy(s => s.Title),
			enableTracking: false,
			splitQuery: true,
			ct: ct);

		var storeDTO = mapper.Map<PaginatedListDTO<StoreDTO>>(stores);
		return storeDTO;
	}
}
