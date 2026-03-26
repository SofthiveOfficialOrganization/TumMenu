using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sitemaps.Queries;

public class GetSitemapDataQueryHandler(
    IRepository<Store> storeRepository,
    IRepository<Category> categoryRepository,
    IRepository<Product> productRepository) : IRequestHandler<GetSitemapDataQuery, SitemapDataDTO>
{
    public async Task<SitemapDataDTO> Handle(GetSitemapDataQuery request, CancellationToken ct)
    {
        var result = new SitemapDataDTO();

        // 1. Stores
        var stores = await storeRepository.Query(tracked: false)
            .Include(s => s.Company)
            .Where(s => !s.IsDeleted && !s.Company.IsDeleted)
            .Select(s => new SitemapItemDTO
            {
                CompanySlug = s.Company.Slug,
                StoreSlug = s.Slug,
                LastModified = s.ModifiedAt ?? s.CreatedAt,
                Type = SitemapItemType.Store
            })
            .ToListAsync(ct);

        result.Items.AddRange(stores);

        // 2. Categories
        var categories = await categoryRepository.Query(tracked: false)
            .Include(c => c.Menu).ThenInclude(m => m!.Store).ThenInclude(s => s!.Company)
            .Include(c => c.CategoryLibraryItem)
            .Where(c => c.IsActive && !c.IsDeleted && 
                        c.Menu.Store != null && !c.Menu.Store.IsDeleted &&
                        c.Menu.Store.Company != null && !c.Menu.Store.Company.IsDeleted)
            .Select(c => new SitemapItemDTO
            {
                CompanySlug = c.Menu.Store!.Company.Slug,
                StoreSlug = c.Menu.Store.Slug,
                CategorySlug = c.CategoryLibraryItem.Slug,
                LastModified = c.ModifiedAt ?? c.CreatedAt,
                Type = SitemapItemType.Category
            })
            .ToListAsync(ct);

        result.Items.AddRange(categories);

        // 3. Products
        var products = await productRepository.Query(tracked: false)
            .Include(p => p.Category).ThenInclude(c => c.Menu).ThenInclude(m => m!.Store).ThenInclude(s => s!.Company)
            .Include(p => p.Category).ThenInclude(c => c.CategoryLibraryItem)
            .Where(p => p.IsActive && !p.IsDeleted &&
                        p.Category.IsActive && !p.Category.IsDeleted &&
                        p.Category.Menu.Store != null && !p.Category.Menu.Store.IsDeleted &&
                        p.Category.Menu.Store.Company != null && !p.Category.Menu.Store.Company.IsDeleted)
            .Select(p => new SitemapItemDTO
            {
                CompanySlug = p.Category.Menu.Store!.Company.Slug,
                StoreSlug = p.Category.Menu.Store.Slug,
                CategorySlug = p.Category.CategoryLibraryItem.Slug,
                ProductSlug = p.Slug,
                LastModified = p.ModifiedAt ?? p.CreatedAt,
                Type = SitemapItemType.Product
            })
            .ToListAsync(ct);

        result.Items.AddRange(products);

        return result;
    }
}
