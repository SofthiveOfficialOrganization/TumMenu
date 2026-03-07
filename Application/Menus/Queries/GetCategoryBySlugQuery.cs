using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public sealed record GetCategoryBySlugQuery(
	string CompanySlug,
	string StoreSlug,
	string CategorySlug
) : IRequest<CategoryPageDTO>;

public sealed class CategoryPageDTO
{
	public string CompanySlug { get; set; } = null!;
	public string StoreSlug { get; set; } = null!;
	public string StoreName { get; set; } = null!;
	public string CompanyName { get; set; } = null!;
	public string CategorySlug { get; set; } = null!;
	public string CategoryTitle { get; set; } = null!;
	public string? CategoryDescription { get; set; }
	public List<CategoryProductItemDTO> Products { get; set; } = [];
	public List<CategorySubCategoryItemDTO> SubCategories { get; set; } = [];
}

public sealed class CategorySubCategoryItemDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? IconKey { get; set; }
	public int ProductCount { get; set; }
}

public sealed class CategoryProductItemDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? Description { get; set; }
	public decimal BasePrice { get; set; }
	public bool? IsVegan { get; set; }
	public bool? IsVegetarian { get; set; }
	public int? EstimatedPreparationTimeInMinutes { get; set; }
	public string? Allergens { get; set; }
	public string? ImageUrl { get; set; }
}

public class GetCategoryBySlugHandler(
	IRepository<Store> repoStore
) : IRequestHandler<GetCategoryBySlugQuery, CategoryPageDTO>
{
	public async Task<CategoryPageDTO> Handle(GetCategoryBySlugQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query()
			.AsSplitQuery()
			.Include(s => s.Company)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.CategoryLibraryItem)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.Products.Where(p => p.IsActive))
				.ThenInclude(p => p.Medias)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active)) // Include SubCategories
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.SubCategories.Where(sc => sc.IsActive))
				.ThenInclude(sc => sc.CategoryLibraryItem)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active)) // Include Products of SubCategories for count
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.SubCategories.Where(sc => sc.IsActive))
				.ThenInclude(sc => sc.Products.Where(p => p.IsActive))
			.FirstOrDefaultAsync(
				s => s.Slug == req.StoreSlug && s.Company.Slug == req.CompanySlug,
				ct
			);

		if (store is null)
			throw new NotFoundAppException("Dükkan bulunamadı.");

		var menu = store.Menus.FirstOrDefault();
		if (menu is null)
			throw new NotFoundAppException("Bu dükkan için aktif bir menü bulunamadı.");

		var category = menu.Categories
			.FirstOrDefault(c => c.IsActive && c.CategoryLibraryItem.Slug == req.CategorySlug);

		if (category is null)
			throw new NotFoundAppException("Kategori bulunamadı.");

		return new CategoryPageDTO
		{
			CompanySlug = store.Company.Slug,
			StoreSlug = store.Slug,
			StoreName = store.Title,
			CompanyName = store.Company.Title,
			CategorySlug = category.CategoryLibraryItem.Slug,
			CategoryTitle = category.CategoryLibraryItem.Title,
			CategoryDescription = category.CategoryLibraryItem.Description,
			Products = category.Products
				.Where(p => p.IsActive)
				.OrderBy(p => p.SortOrder)
				.Select(p => new CategoryProductItemDTO
				{
					Id = p.Id,
					Title = p.Title,
					Slug = p.Slug,
					Description = p.Description,
					BasePrice = p.BasePrice,
					IsVegan = p.IsVegan,
					IsVegetarian = p.IsVegetarian,
					EstimatedPreparationTimeInMinutes = p.EstimatedPreparationTimeInMinutes,
					Allergens = p.Allergens,
					ImageUrl = p.Medias
						.Where(m => m.Kind == MediaKind.Image)
						.OrderBy(m => m.SortOrder)
						.Select(m => m.MediaUrl)
						.FirstOrDefault()
				})
				.ToList(),
			SubCategories = category.SubCategories
				.Where(sc => sc.IsActive)
				.OrderBy(sc => sc.SortOrder)
				.Select(sc => new CategorySubCategoryItemDTO
				{
					Title = sc.CategoryLibraryItem.Title,
					Slug = sc.CategoryLibraryItem.Slug,
					IconKey = sc.CategoryLibraryItem.IconKey,
					ProductCount = sc.Products.Count(p => p.IsActive)
				})
				.ToList()
		};
	}
}
