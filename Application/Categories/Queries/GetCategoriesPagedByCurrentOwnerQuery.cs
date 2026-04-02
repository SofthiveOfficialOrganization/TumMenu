using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Application.Common.Base.Page;

namespace Application.Categories.Queries;

public class GetCategoriesPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<CategoryListDTO>>, IAuthorizedRequest
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? MenuId { get; set; }
}

public class GetCategoriesByCurrentOwnerHandler(
    IRepository<Category> repoCategory,
    IRepository<Company> repoCompany,
    IRepository<Store> repoStore,
    IRepository<Menu> repoMenu,
    IMapper mapper,
    IUserContext userContext
) : IRequestHandler<GetCategoriesPagedByCurrentOwnerQuery, PaginatedListDTO<CategoryListDTO>>
{
    public async Task<PaginatedListDTO<CategoryListDTO>> Handle(GetCategoriesPagedByCurrentOwnerQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        var isAdmin = userContext.IsAdmin;
        var contextCompanyId = userContext.CompanyIdParsed;

        var paginate = await repoCategory.GetPageListAsync(
            req,
            c =>
                (isAdmin ||
                    (contextCompanyId.HasValue
                        ? (c.Menu.CompanyId == contextCompanyId.Value || (c.Menu.StoreId != null && c.Menu.Store!.CompanyId == contextCompanyId.Value))
                        : ((c.Menu.CompanyId != null && c.Menu.Company!.Owner!.ApplicationUserId == userId) ||
                           (c.Menu.StoreId != null && c.Menu.Store!.Company.Owner!.ApplicationUserId == userId)))) &&
                (string.IsNullOrEmpty(req.Search) || c.CategoryLibraryItem.Title.Contains(req.Search)) &&
                (!req.CompanyId.HasValue || c.Menu.CompanyId == req.CompanyId.Value || (c.Menu.StoreId != null && c.Menu.Store!.CompanyId == req.CompanyId.Value)) &&
                (!req.StoreId.HasValue || c.Menu.StoreId == req.StoreId.Value) &&
                (!req.MenuId.HasValue || c.MenuId == req.MenuId.Value),
            orderBy: c => c.OrderByDescending(x => x.CreatedAt),
            enableTracking: false,
            ct: ct
        );

        var response = mapper.Map<PaginatedListDTO<CategoryListDTO>>(paginate);

        if (req.CompanyId.HasValue)
        {
            var company = await repoCompany.GetByIdAsync(req.CompanyId.Value, ct);
            if (company != null) response.FilterNames[req.CompanyId.ToString()!] = company.Title;
        }
        if (req.StoreId.HasValue)
        {
            var store = await repoStore.GetByIdAsync(req.StoreId.Value, ct);
            if (store != null) response.FilterNames[req.StoreId.ToString()!] = store.Title;
        }
        if (req.MenuId.HasValue)
        {
            var menu = await repoMenu.GetByIdAsync(req.MenuId.Value, ct);
            if (menu != null) response.FilterNames[req.MenuId.ToString()!] = menu.Title;
        }

        return response;
    }
}
