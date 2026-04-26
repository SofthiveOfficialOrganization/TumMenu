using Application.Abstractions;
using Application.Common.Exceptions;
using Application.MenuDesigns.DTOs;
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
	public bool? IsVegan { get; set; }
	public bool? IsVegetarian { get; set; }
	public int? EstimatedPreparationTimeInMinutes { get; set; }
	public string? Allergens { get; set; }
	public List<string> ImageUrls { get; set; } = [];
	public MenuDesignDTO? MenuDesign { get; set; }
}

public class GetProductBySlugHandler(
	IRepository<Store> repoStore
) : IRequestHandler<GetProductBySlugQuery, ProductPageDTO>
{
	public async Task<ProductPageDTO> Handle(GetProductBySlugQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query()
			.AsSplitQuery()
			.Include(s => s.Company)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.MenuDesign)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.CategoryLibraryItem)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.Products.Where(p => p.IsActive))
				.ThenInclude(p => p.Medias)
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
			IsVegan = product.IsVegan,
			IsVegetarian = product.IsVegetarian,
			EstimatedPreparationTimeInMinutes = product.EstimatedPreparationTimeInMinutes,
			Allergens = product.Allergens,
			ImageUrls = product.Medias
				.Where(m => m.Kind == MediaKind.Image)
				.OrderBy(m => m.SortOrder)
				.Select(m => m.MediaUrl)
				.ToList(),
			MenuDesign = menu.MenuDesign is null ? null : new MenuDesignDTO
			{
				Id = menu.MenuDesign.Id,
				Name = menu.MenuDesign.Name,
				Slug = menu.MenuDesign.Slug,
				PrimaryColor = menu.MenuDesign.PrimaryColor,
				PrimaryDarkColor = menu.MenuDesign.PrimaryDarkColor,
				AccentColor = menu.MenuDesign.AccentColor,
				BackgroundColor = menu.MenuDesign.BackgroundColor,
				SurfaceColor = menu.MenuDesign.SurfaceColor,
				TextColor = menu.MenuDesign.TextColor,
				MutedColor = menu.MenuDesign.MutedColor,
				BorderRadius = menu.MenuDesign.BorderRadius,
				BackgroundGradient = menu.MenuDesign.BackgroundGradient,
				PreviewImageUrl = menu.MenuDesign.PreviewImageUrl,
				IsDefault = menu.MenuDesign.IsDefault,
				SortOrder = menu.MenuDesign.SortOrder
			}
		};
	}
}
