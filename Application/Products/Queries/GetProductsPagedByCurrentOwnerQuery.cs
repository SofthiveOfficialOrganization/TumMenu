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
                .Include(p => p.Category).ThenInclude(c => c.CategoryLibraryItem)
                .Include(p => p.Category).ThenInclude(c => c.Menu).ThenInclude(m => m.Company)
                .Include(p => p.Category).ThenInclude(c => c.Menu).ThenInclude(m => m.Store),
            enableTracking: false,
            ct: ct
        );
        
        var productListDTO = mapper.Map<PaginatedListDTO<ProductListDTO>>(productList);
        return productListDTO;
    }
}
