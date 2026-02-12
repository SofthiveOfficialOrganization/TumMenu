using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Stores.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries;

public class GetStoresPagedQuery : PageRequest, IRequest<PaginatedListDTO<StoreDTO>>
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
}

public class GetStoresPagedHandler(
    IRepository<Store> repoStore,
    IUserContext userContext,
    IMapper mapper
) : IRequestHandler<GetStoresPagedQuery, PaginatedListDTO<StoreDTO>>
{
    public async Task<PaginatedListDTO<StoreDTO>> Handle(GetStoresPagedQuery req, CancellationToken ct)
    {
        var appUserId = userContext.UserId;
        var roles = userContext.Roles;
        var isAdmin = roles.Contains("Admin");

        var stores = await repoStore.GetPageListAsync(
            req,
            s => (string.IsNullOrEmpty(req.Search) || s.Title.Contains(req.Search) || s.Slug.Contains(req.Search)) &&
                 (!req.CompanyId.HasValue || s.CompanyId == req.CompanyId) &&
                 (isAdmin || (s.Company.Owner != null && s.Company.Owner.ApplicationUserId == appUserId)),
            include: s => s.Include(x => x.Company).ThenInclude(x => x.Owner),
            orderBy: s => s.OrderBy(x => x.Title),
            ct: ct
        );

        return mapper.Map<PaginatedListDTO<StoreDTO>>(stores);
    }
}
