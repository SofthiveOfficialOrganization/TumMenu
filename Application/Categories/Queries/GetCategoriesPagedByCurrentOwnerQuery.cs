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
        var query = repoCategory.Query(tracked: false);

        // 1. Authorization Optimization
        if (!userContext.Roles.Contains("Admin"))
        {
            // Prefer using CompanyId from claims to avoid joining Owner/ApplicationUser tables
            if (Guid.TryParse(userContext.CompanyId, out var companyId))
            {
                query = query.Where(c => c.Menu.CompanyId == companyId || (c.Menu.StoreId != null && c.Menu.Store!.CompanyId == companyId));
            }
            else
            {
                // Fallback authorization (still optimized by Projection later)
                query = query.Where(c => (c.Menu.CompanyId != null && c.Menu.Company!.Owner!.ApplicationUserId == userId) ||
                                         (c.Menu.StoreId != null && c.Menu.Store!.Company.Owner!.ApplicationUserId == userId));
            }
        }

        // 2. Filters
        if (!string.IsNullOrEmpty(req.Search))
            query = query.Where(c => c.CategoryLibraryItem.Title.Contains(req.Search));

        if (req.CompanyId.HasValue)
            query = query.Where(c => c.Menu.CompanyId == req.CompanyId.Value || (c.Menu.StoreId != null && c.Menu.Store!.CompanyId == req.CompanyId.Value));

        if (req.StoreId.HasValue)
            query = query.Where(c => c.Menu.StoreId == req.StoreId.Value);

        if (req.MenuId.HasValue)
            query = query.Where(c => c.MenuId == req.MenuId.Value);

        // 3. Sorting
        query = query.OrderByDescending(c => c.CreatedAt);

        // 4. Projection & Pagination (Mapster ProjectToType replaces Include/ThenInclude)
        var paginate = await query
            .ProjectToType<CategoryListDTO>(mapper.Config)
            .ToPaginateAsync(ct, req.Page, req.PageSize, req.From);

        return new PaginatedListDTO<CategoryListDTO>
        {
            Items = paginate.Items,
            Index = paginate.Index,
            Size = paginate.Size,
            Count = paginate.Count,
            From = paginate.From,
            Pages = paginate.Pages,
            HasPrevious = paginate.HasPrevious,
            HasNext = paginate.HasNext
        };
    }
}
