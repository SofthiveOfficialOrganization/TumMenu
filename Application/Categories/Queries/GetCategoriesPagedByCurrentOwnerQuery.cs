using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public class GetCategoriesPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<CategoryListDTO>>
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? MenuId { get; set; }
}

public class GetCategoriesByCurrentOwnerHandler(
    IRepository<Category> repoCategory,
    IMapper mapper,
    IUserContext userContext
) : IRequestHandler<GetCategoriesPagedByCurrentOwnerQuery, PaginatedListDTO<CategoryListDTO>>
{
    public async Task<PaginatedListDTO<CategoryListDTO>> Handle(GetCategoriesPagedByCurrentOwnerQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        var categoryList = await repoCategory.GetPageListAsync(
            req,
             c => ((c.Menu.CompanyId != null && c.Menu.Company!.Owner!.ApplicationUserId == userId) || 
                  (c.Menu.StoreId != null && c.Menu.Store!.Company.Owner!.ApplicationUserId == userId)) &&
                 (string.IsNullOrEmpty(req.Search) || c.CategoryLibraryItem.Title.Contains(req.Search)) &&
                 (!req.CompanyId.HasValue || c.Menu.CompanyId == req.CompanyId.Value) &&
                 (!req.StoreId.HasValue || c.Menu.StoreId == req.StoreId.Value) &&
                 (!req.MenuId.HasValue || c.MenuId == req.MenuId.Value),
            orderBy: query => query.OrderByDescending(c => c.CreatedAt),
            include: query => query
                .Include(c => c.CategoryLibraryItem)
                .Include(c => c.Menu)
                    .ThenInclude(m => m.Company)
                .Include(c => c.Menu)
                    .ThenInclude(m => m.Store)
                .Include(c => c.Products),
            ct: ct
        );
        var categoryListDTO = mapper.Map<PaginatedListDTO<CategoryListDTO>>(categoryList);
        return categoryListDTO;
    }
}
