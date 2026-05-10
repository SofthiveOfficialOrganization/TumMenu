using Application.Abstractions;
using Application.Common.Exceptions;
using Application.MenuDesigns.DTOs;
using Application.Products.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public sealed record GetProductBySlugQuery(
	string CompanySlug,
	string StoreSlug,
	string CategorySlug,
	string ProductSlug
) : IRequest<ProductPageDTO>;

public sealed class ProductPageDTO
{
	public string CompanySlug { get; set; } = null!;
	public string StoreSlug { get; set; } = null!;
	public string StoreName { get; set; } = null!;
	public string CompanyName { get; set; } = null!;
	public string CategorySlug { get; set; } = null!;
	public string CategoryTitle { get; set; } = null!;
	public Guid ProductId { get; set; }
	public string ProductTitle { get; set; } = null!;
	public string ProductSlug { get; set; } = null!;
	public string? ProductDescription { get; set; }
	public decimal BasePrice { get; set; }
	public List<ProductPriceDTO> Prices { get; set; } = [];
	public bool? IsVegan { get; set; }
	public bool? IsVegetarian { get; set; }
	public int? EstimatedPreparationTimeInMinutes { get; set; }
	public string? Allergens { get; set; }
	public List<string> ImageUrls { get; set; } = [];
	public MenuDesignDTO? MenuDesign { get; set; }
	public bool ShowRepresentativeImagesDisclaimer { get; set; }

	// Görünürlük kontrolleri
	public bool ShowPricesOnMenu { get; set; } = true;   // Menüde fiyatlar gösterilsin mi?
	public bool ShowMenuButton { get; set; } = true;    // Dükkan detay sayfasında menü butonu
}

public class GetProductBySlugHandler(
	IRepository<Store> repoStore,
	IRepository<Menu> repoMenu,
	IRepository<Category> repoCategory
) : IRequestHandler<GetProductBySlugQuery, ProductPageDTO>
{
	public async Task<ProductPageDTO> Handle(GetProductBySlugQuery req, CancellationToken ct)
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

		var category = await repoCategory.Query()
			.AsSplitQuery()
			.Include(c => c.Menu)
				.ThenInclude(m => m.MenuDesign)
			.Include(c => c.CategoryLibraryItem)
			.Include(c => c.Products.Where(p => p.IsActive))
				.ThenInclude(p => p.Medias)
			.Include(c => c.Products.Where(p => p.IsActive))
				.ThenInclude(p => p.Prices)
			.FirstOrDefaultAsync(
				c => c.IsActive &&
				     c.MenuId == menuId.Value &&
				     c.CategoryLibraryItem.Slug == req.CategorySlug,
				ct);

		if (category is null)
			throw new NotFoundAppException("Kategori bulunamadı.");

		var product = category.Products
			.FirstOrDefault(p => p.IsActive && p.Slug == req.ProductSlug);

		if (product is null)
			throw new NotFoundAppException("Ürün bulunamadı.");

		return new ProductPageDTO
		{
			CompanySlug = store.Company.Slug,
			StoreSlug = store.Slug,
			StoreName = store.Title,
			CompanyName = store.Company.Title,
			CategorySlug = category.CategoryLibraryItem.Slug,
			CategoryTitle = category.CategoryLibraryItem.Title,
			ProductId = product.Id,
			ProductTitle = product.Title,
			ProductSlug = product.Slug,
			ProductDescription = product.Description,
			BasePrice = product.BasePrice,
			Prices = product.Prices
				.OrderBy(p => p.CreatedAt)
				.Select(p => new ProductPriceDTO
				{
					Id = p.Id,
					Size = p.Size,
					Price = p.Price,
					CreatedAt = p.CreatedAt,
					ModifiedAt = p.ModifiedAt
				})
				.ToList(),
			IsVegan = product.IsVegan,
			IsVegetarian = product.IsVegetarian,
			EstimatedPreparationTimeInMinutes = product.EstimatedPreparationTimeInMinutes,
			Allergens = product.Allergens,
			ImageUrls = product.Medias
				.Where(m => m.Kind == MediaKind.Image)
				.OrderBy(m => m.SortOrder)
				.Select(m => m.MediaUrl)
				.ToList(),
			MenuDesign = category.Menu.MenuDesign?.ToDto(),
			ShowRepresentativeImagesDisclaimer = store.ShowRepresentativeImagesDisclaimer,
			ShowPricesOnMenu = store.ShowPricesOnMenu,
			ShowMenuButton = store.ShowMenuButton
		};
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
