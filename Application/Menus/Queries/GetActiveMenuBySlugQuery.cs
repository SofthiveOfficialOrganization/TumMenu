using Application.Abstractions;
using Application.Common.Exceptions;
using Application.MenuDesigns.DTOs;
using Application.Menus.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public sealed record GetActiveMenuBySlugQuery(
	string CompanySlug,
	string StoreSlug
) : IRequest<MenuDTO>;

public class GetActiveMenuBySlugHandler(
	IRepository<Store> repoStore,
	IRepository<Menu> repoMenu
) : IRequestHandler<GetActiveMenuBySlugQuery, MenuDTO>
{
	public async Task<MenuDTO> Handle(GetActiveMenuBySlugQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query()
			.AsSplitQuery()
			.Include(s => s.Company)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.MenuDesign)
			.Include(s => s.Menus.Where(m => m.Status == MenuStatus.Active))
				.ThenInclude(m => m.Categories.Where(c => c.IsActive))
				.ThenInclude(c => c.CategoryLibraryItem)
					.ThenInclude(cli => cli.Medias)
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

		var storeMenu = store.Menus.FirstOrDefault();

		if (storeMenu is not null)
			return BuildMenuDTO(storeMenu, store.Title, store.Company.Title);

		Menu? companyMenu = null;

		if (store.Company.DefaultMainMenuId.HasValue)
		{
			companyMenu = await repoMenu.Query()
				.AsSplitQuery()
				.Include(m => m.Company)
				.Include(m => m.MenuDesign)
				.Include(m => m.Categories.Where(c => c.IsActive))
					.ThenInclude(c => c.CategoryLibraryItem)
						.ThenInclude(cli => cli.Medias)
				.Include(m => m.Categories.Where(c => c.IsActive))
					.ThenInclude(c => c.Products.Where(p => p.IsActive))
						.ThenInclude(p => p.Medias)
				.FirstOrDefaultAsync(
					m => m.Id == store.Company.DefaultMainMenuId.Value &&
						m.CompanyId == store.Company.Id &&
						m.Status == MenuStatus.MainMenu,
					ct
				);
		}

		// Fallback: use the company's default main menu, then the oldest available main menu
		if (companyMenu is null)
		{
			companyMenu = await repoMenu.Query()
				.AsSplitQuery()
				.Include(m => m.Company)
				.Include(m => m.MenuDesign)
				.Include(m => m.Categories.Where(c => c.IsActive))
					.ThenInclude(c => c.CategoryLibraryItem)
						.ThenInclude(cli => cli.Medias)
				.Include(m => m.Categories.Where(c => c.IsActive))
					.ThenInclude(c => c.Products.Where(p => p.IsActive))
						.ThenInclude(p => p.Medias)
				.OrderBy(m => m.CreatedAt)
				.FirstOrDefaultAsync(
					m => m.CompanyId == store.Company.Id && m.Status == MenuStatus.MainMenu,
					ct
				);
		}

		if (companyMenu is null)
			throw new NotFoundAppException("Bu dükkan için aktif bir menü bulunamadı.");

		return BuildMenuDTO(companyMenu, store.Title, store.Company.Title);
	}

	private static MenuDTO BuildMenuDTO(Menu menu, string storeName, string companyName)
	{
		return new MenuDTO
		{
			Id = menu.Id,
			Title = menu.Title,
			StoreId = menu.StoreId,
			CompanyId = menu.CompanyId,
			MenuDesignId = menu.MenuDesignId,
			MenuDesign = menu.MenuDesign?.ToDto(),
			Status = menu.Status,
			StoreName = storeName,
			CompanyName = companyName,
			IsDefaultCompanyMenu = menu.Company != null && menu.Company.DefaultMainMenuId == menu.Id,
			Categories = menu.Categories
				.Where(c => c.ParentId == null) // ONLY RETURN ROOT CATEGORIES
				.OrderBy(c => c.SortOrder)
				.Select(c => new Application.Categories.DTOs.CategoryDTO
				{
					Id = c.Id,
					MenuId = c.MenuId,
					CategoryLibraryItemId = c.CategoryLibraryItemId,
					ParentId = c.ParentId,
					SortOrder = c.SortOrder,
					IsActive = c.IsActive,
					CategoryLibraryItem = new Application.Categories.DTOs.CategoryLibraryItemDTO
					{
						Id = c.CategoryLibraryItem.Id,
						Title = c.CategoryLibraryItem.Title,
						Slug = c.CategoryLibraryItem.Slug,
						Description = c.CategoryLibraryItem.Description,
						IconKey = c.CategoryLibraryItem.IconKey,
						ParentId = c.CategoryLibraryItem.ParentId,
						ImageUrl = c.CategoryLibraryItem.Medias
							.Where(m => m.Kind == MediaKind.Image)
							.OrderBy(m => m.SortOrder)
							.Select(m => m.MediaUrl)
							.FirstOrDefault()
					},
					Products = c.Products
						.OrderBy(p => p.SortOrder)
						.Select(p => new Application.Products.DTOs.ProductDTO
						{
							Id = p.Id,
							Title = p.Title,
							Description = p.Description,
							CategoryId = p.CategoryId,
							BasePrice = p.BasePrice,
							SortOrder = p.SortOrder,
							IsActive = p.IsActive,
							Allergens = p.Allergens,
							IsVegan = p.IsVegan,
							IsVegetarian = p.IsVegetarian,
							EstimatedPreparationTimeInMinutes = p.EstimatedPreparationTimeInMinutes,
						})
						.ToList()
				})
				.ToList()
		};
	}
}
