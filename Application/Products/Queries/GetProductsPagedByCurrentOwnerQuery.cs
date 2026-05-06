using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Products.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Queries;

public sealed class GetProductsPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<ProductListDTO>>
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? MenuId { get; set; }
    public Guid? CategoryId { get; set; }
}

public class GetProductsPagedByCurrentOwnerHandler(
    IRepository<Product> repoProduct,
    IRepository<Company> repoCompany,
    IRepository<Store> repoStore,
    IRepository<Menu> repoMenu,
    IRepository<Category> repoCategory,
    IMapper mapper,
    IUserContext userContext
) : IRequestHandler<GetProductsPagedByCurrentOwnerQuery, PaginatedListDTO<ProductListDTO>>
{
    public async Task<PaginatedListDTO<ProductListDTO>> Handle(GetProductsPagedByCurrentOwnerQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        
        var productList = await repoProduct.GetPageListAsync(
            req,
            p => ((p.Category.Menu.CompanyId != null && p.Category.Menu.Company!.Owner!.ApplicationUserId == userId) || 
                  (p.Category.Menu.StoreId != null && p.Category.Menu.Store!.Company.Owner!.ApplicationUserId == userId)) &&
                 (string.IsNullOrEmpty(req.Search) || p.Title.Contains(req.Search)) &&
                 (!req.CompanyId.HasValue || p.Category.Menu.CompanyId == req.CompanyId.Value) &&
                 (!req.StoreId.HasValue || p.Category.Menu.StoreId == req.StoreId.Value) &&
                 (!req.MenuId.HasValue || p.Category.MenuId == req.MenuId.Value) &&
                 (!req.CategoryId.HasValue || p.CategoryId == req.CategoryId.Value),
            orderBy: p => p.OrderBy(p => p.Category.SortOrder).ThenBy(p => p.SortOrder),
            include: query => query
                .Include(p => p.Prices)
                .Include(p => p.Category).ThenInclude(c => c.CategoryLibraryItem)
                .Include(p => p.Category).ThenInclude(c => c.Menu).ThenInclude(m => m.Company)
                .Include(p => p.Category).ThenInclude(c => c.Menu).ThenInclude(m => m.Store),
            enableTracking: false,
            ct: ct
        );
        
        var productListDTO = mapper.Map<PaginatedListDTO<ProductListDTO>>(productList);

        // Populate FilterNames for Select2 placeholders
        if (req.CompanyId.HasValue)
        {
            var company = await repoCompany.GetByIdAsync(req.CompanyId.Value, ct);
            if (company != null) productListDTO.FilterNames[req.CompanyId.ToString()!] = company.Title;
        }
        if (req.StoreId.HasValue)
        {
            var store = await repoStore.GetByIdAsync(req.StoreId.Value, ct);
            if (store != null) productListDTO.FilterNames[req.StoreId.ToString()!] = store.Title;
        }
        if (req.MenuId.HasValue)
        {
            var menu = await repoMenu.GetByIdAsync(req.MenuId.Value, ct);
            if (menu != null) productListDTO.FilterNames[req.MenuId.ToString()!] = menu.Title;
        }
        if (req.CategoryId.HasValue)
        {
            var category = await repoCategory.Query(tracked: false)
                .Include(c => c.CategoryLibraryItem)
                .FirstOrDefaultAsync(c => c.Id == req.CategoryId.Value, ct);
            if (category != null) productListDTO.FilterNames[req.CategoryId.ToString()!] = category.CategoryLibraryItem.Title;
        }

        return productListDTO;
    }
}
