using Application.Abstractions;
using Application.Common.Exceptions;
using Application.MenuDesigns.DTOs;
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
	public Guid CategoryId { get; set; }
	public string? CategoryDescription { get; set; }
	public string? ParentCategorySlug { get; set; }
	public List<CategoryProductItemDTO> Products { get; set; } = [];
	public List<CategorySubCategoryItemDTO> SubCategories { get; set; } = [];
	public MenuDesignDTO? MenuDesign { get; set; }
}

public sealed class CategorySubCategoryItemDTO
{
	public string Title { get; set; } = null!;
	public string Slug { get; set; } = null!;
	public string? IconKey { get; set; }
	public string? ImageUrl { get; set; }
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
	IRepository<Store> repoStore,
	IRepository<Menu> repoMenu,
	IRepository<Category> repoCategory
) : IRequestHandler<GetCategoryBySlugQuery, CategoryPageDTO>
{
	public async Task<CategoryPageDTO> Handle(GetCategoryBySlugQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query()
			.Include(s => s.Company)
			.FirstOrDefaultAsync(
				s => s.Slug == req.StoreSlug && s.Company.Slug == req.CompanySlug,
				ct);

		if (store is null)
			throw new NotFoundAppException("Dükkan bulunamadı.");

		var menuId = await ResolveMenuIdAsync(store, ct);

		if (menuId is null)
			throw new NotFoundAppException("Bu dükkan için aktif bir menü bulunamadı.");

		var categoryData = await repoCategory.Query()
			.Where(c => c.IsActive &&
			           c.CategoryLibraryItem.Slug == req.CategorySlug &&
			           c.MenuId == menuId.Value)
			.Select(c => new CategoryPageDTO
			{
				CompanySlug = store.Company.Slug,
				StoreSlug = store.Slug,
				StoreName = store.Title,
				CompanyName = store.Company.Title,
				CategorySlug = c.CategoryLibraryItem.Slug,
				CategoryTitle = c.CategoryLibraryItem.Title,
				CategoryId = c.Id,
				CategoryDescription = c.CategoryLibraryItem.Description,
				ParentCategorySlug = c.Parent != null ? c.Parent.CategoryLibraryItem.Slug : null,
				MenuDesign = c.Menu.MenuDesign == null ? null : c.Menu.MenuDesign.ToDto(),
				Products = c.Products
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
				SubCategories = c.SubCategories
					.Where(sc => sc.IsActive)
					.OrderBy(sc => sc.SortOrder)
					.Select(sc => new CategorySubCategoryItemDTO
					{
						Title = sc.CategoryLibraryItem.Title,
						Slug = sc.CategoryLibraryItem.Slug,
						IconKey = sc.CategoryLibraryItem.IconKey,
						ProductCount = sc.Products.Count(p => p.IsActive),
						ImageUrl = sc.CategoryLibraryItem.Medias
							.Where(m => m.Kind == MediaKind.Image)
							.OrderBy(m => m.SortOrder)
							.Select(m => m.MediaUrl)
							.FirstOrDefault()
					})
					.ToList()
			})
			.FirstOrDefaultAsync(ct);

		if (categoryData is null)
			throw new NotFoundAppException("Kategori bulunamadı.");

		return categoryData;
	}

	private async Task<Guid?> ResolveMenuIdAsync(Store store, CancellationToken ct)
	{
		var activeStoreMenuId = await repoMenu.Query()
			.Where(m => m.StoreId == store.Id && m.Status == MenuStatus.Active)
			.OrderBy(m => m.CreatedAt)
			.Select(m => (Guid?)m.Id)
			.FirstOrDefaultAsync(ct);

		if (activeStoreMenuId.HasValue)
			return activeStoreMenuId.Value;

		if (store.Company.DefaultMainMenuId.HasValue)
		{
			var defaultMainMenuId = await repoMenu.Query()
				.Where(m => m.Id == store.Company.DefaultMainMenuId.Value &&
				            m.CompanyId == store.Company.Id &&
				            m.Status == MenuStatus.MainMenu)
				.Select(m => (Guid?)m.Id)
				.FirstOrDefaultAsync(ct);

			if (defaultMainMenuId.HasValue)
				return defaultMainMenuId.Value;
		}

		return await repoMenu.Query()
			.Where(m => m.CompanyId == store.Company.Id && m.Status == MenuStatus.MainMenu)
			.OrderBy(m => m.CreatedAt)
			.Select(m => (Guid?)m.Id)
			.FirstOrDefaultAsync(ct);
	}
}
