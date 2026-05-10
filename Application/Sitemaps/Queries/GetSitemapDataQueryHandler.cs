using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sitemaps.Queries;

public class GetSitemapDataQueryHandler(
    IRepository<Store> storeRepository,
    IRepository<Category> categoryRepository,
    IRepository<Product> productRepository,
    IRepository<BlogPost> blogPostRepository) : IRequestHandler<GetSitemapDataQuery, SitemapDataDTO>
{
    public async Task<SitemapDataDTO> Handle(GetSitemapDataQuery request, CancellationToken ct)
    {
        var result = new SitemapDataDTO();

        // 1. Stores
        var stores = await storeRepository.Query(tracked: false)
            .Include(s => s.Company)
            .Where(s => !s.IsDeleted && !s.Company.IsDeleted &&
                        s.ShowInSearchAndListings)
            .Where(s => !EF.Functions.Like(s.Slug, "%test%") &&
                        !EF.Functions.Like(s.Title, "%test%") &&
                        !EF.Functions.Like(s.Company.Slug, "%test%") &&
                        !EF.Functions.Like(s.Company.Title, "%test%") &&
                        !EF.Functions.Like(s.Slug, "%deneme%") &&
                        !EF.Functions.Like(s.Title, "%deneme%") &&
                        !EF.Functions.Like(s.Company.Slug, "%deneme%") &&
                        !EF.Functions.Like(s.Company.Title, "%deneme%"))
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
                        c.Menu.Store.ShowInSearchAndListings &&
                        c.Menu.Store.Company != null && !c.Menu.Store.Company.IsDeleted)
            .Where(c => !EF.Functions.Like(c.Menu.Store!.Slug, "%test%") &&
                        !EF.Functions.Like(c.Menu.Store.Title, "%test%") &&
                        !EF.Functions.Like(c.Menu.Store.Company.Slug, "%test%") &&
                        !EF.Functions.Like(c.Menu.Store.Company.Title, "%test%") &&
                        !EF.Functions.Like(c.CategoryLibraryItem.Slug, "%test%") &&
                        !EF.Functions.Like(c.CategoryLibraryItem.Title, "%test%") &&
                        !EF.Functions.Like(c.Menu.Store.Slug, "%deneme%") &&
                        !EF.Functions.Like(c.Menu.Store.Title, "%deneme%") &&
                        !EF.Functions.Like(c.CategoryLibraryItem.Slug, "%deneme%") &&
                        !EF.Functions.Like(c.CategoryLibraryItem.Title, "%deneme%") &&
                        (c.Products.Any(p => p.IsActive && !p.IsDeleted &&
                                             !EF.Functions.Like(p.Slug, "%test%") &&
                                             !EF.Functions.Like(p.Title, "%test%") &&
                                             !EF.Functions.Like(p.Slug, "%deneme%") &&
                                             !EF.Functions.Like(p.Title, "%deneme%")) ||
                         c.SubCategories.Any(sc => sc.IsActive && !sc.IsDeleted)))
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
                        p.Category.Menu.Store.ShowInSearchAndListings &&
                        p.Category.Menu.Store.Company != null && !p.Category.Menu.Store.Company.IsDeleted)
            .Where(p => !EF.Functions.Like(p.Category.Menu.Store!.Slug, "%test%") &&
                        !EF.Functions.Like(p.Category.Menu.Store.Title, "%test%") &&
                        !EF.Functions.Like(p.Category.Menu.Store.Company.Slug, "%test%") &&
                        !EF.Functions.Like(p.Category.Menu.Store.Company.Title, "%test%") &&
                        !EF.Functions.Like(p.Category.CategoryLibraryItem.Slug, "%test%") &&
                        !EF.Functions.Like(p.Category.CategoryLibraryItem.Title, "%test%") &&
                        !EF.Functions.Like(p.Slug, "%test%") &&
                        !EF.Functions.Like(p.Title, "%test%") &&
                        !EF.Functions.Like(p.Category.Menu.Store.Slug, "%deneme%") &&
                        !EF.Functions.Like(p.Category.Menu.Store.Title, "%deneme%") &&
                        !EF.Functions.Like(p.Slug, "%deneme%") &&
                        !EF.Functions.Like(p.Title, "%deneme%") &&
                        p.Description != null &&
                        p.Description.Length >= 20)
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

        // 4. Blog Posts
        var blogPosts = await blogPostRepository.Query(tracked: false)
            .Where(b => b.IsPublished && !b.IsDeleted)
            .Select(b => new SitemapItemDTO
            {
                BlogSlug = b.Slug,
                LastModified = b.ModifiedAt ?? b.CreatedAt,
                Type = SitemapItemType.BlogPost
            })
            .ToListAsync(ct);

        result.Items.AddRange(blogPosts);

        return result;
    }
}
