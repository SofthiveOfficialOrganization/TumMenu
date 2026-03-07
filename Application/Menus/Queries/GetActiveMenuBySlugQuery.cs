using Application.Abstractions;
using Application.Common.Exceptions;
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
	IRepository<Store> repoStore
) : IRequestHandler<GetActiveMenuBySlugQuery, MenuDTO>
{
	public async Task<MenuDTO> Handle(GetActiveMenuBySlugQuery req, CancellationToken ct)
	{
		var store = await repoStore.Query()
			.Include(s => s.Company)
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

		var dto = new MenuDTO
		{
			Id = menu.Id,
			Title = menu.Title,
			StoreId = menu.StoreId,
			CompanyId = menu.CompanyId,
			Status = menu.Status,
			StoreName = store.Title,
			CompanyName = store.Company.Title,
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

		return dto;
	}
}
