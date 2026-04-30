using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public sealed class SyncMainMenuToStoreMenuCommand : IRequest<MenuDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Ana menü dükkan menüsüne senkronlandı";
	public Guid SourceMenuId { get; set; }
	public Guid StoreId { get; set; }
}

public sealed class SyncMainMenuToStoreMenuCommandHandler(
	IRepository<Menu> repoMenu,
	IRepository<Category> repoCategory,
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<SyncMainMenuToStoreMenuCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(SyncMainMenuToStoreMenuCommand req, CancellationToken ct)
	{
		var source = await repoMenu.Query(tracked: false)
			.AsSplitQuery()
			.Include(m => m.Categories)
				.ThenInclude(c => c.Products)
			.FirstOrDefaultAsync(m => m.Id == req.SourceMenuId, ct)
			?? throw new NotFoundAppException("Kaynak ana menü bulunamadı.");

		var target = await repoMenu.Query(tracked: true)
			.AsSplitQuery()
			.Include(m => m.Categories)
				.ThenInclude(c => c.Products)
			.OrderByDescending(m => m.CreatedAt)
			.FirstOrDefaultAsync(m => m.StoreId == req.StoreId && m.Status == MenuStatus.Active, ct);

		if (target is null)
		{
			target = new Menu
			{
				Id = Guid.NewGuid(),
				Title = source.Title,
				StoreId = req.StoreId,
				MenuDesignId = source.MenuDesignId,
				Status = MenuStatus.Active
			};

			await repoMenu.AddAsync(target, ct);
		}
		else
		{
			target.Title = source.Title;
			target.MenuDesignId = source.MenuDesignId;
		}

		var sourceCategories = source.Categories
			.OrderBy(c => c.ParentId.HasValue ? 1 : 0)
			.ThenBy(c => c.SortOrder)
			.ToList();

		var sourceLibraryIds = sourceCategories
			.Select(c => c.CategoryLibraryItemId)
			.ToHashSet();

		foreach (var staleCategory in target.Categories.Where(c => !sourceLibraryIds.Contains(c.CategoryLibraryItemId)).ToList())
		{
			repoCategory.SoftDelete(staleCategory);
		}

		var targetByLibraryItemId = target.Categories
			.Where(c => sourceLibraryIds.Contains(c.CategoryLibraryItemId))
			.GroupBy(c => c.CategoryLibraryItemId)
			.ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedAt).First());

		var categoryIdMap = new Dictionary<Guid, Guid>();

		foreach (var sourceCategory in sourceCategories)
		{
			var targetCategory = targetByLibraryItemId.GetValueOrDefault(sourceCategory.CategoryLibraryItemId);

			if (targetCategory is null)
			{
				targetCategory = new Category
				{
					Id = Guid.NewGuid(),
					MenuId = target.Id,
					CategoryLibraryItemId = sourceCategory.CategoryLibraryItemId
				};

				await repoCategory.AddAsync(targetCategory, ct);
				targetByLibraryItemId[sourceCategory.CategoryLibraryItemId] = targetCategory;
			}

			targetCategory.SortOrder = sourceCategory.SortOrder;
			targetCategory.IsActive = sourceCategory.IsActive;
			targetCategory.ParentId = sourceCategory.ParentId.HasValue &&
				categoryIdMap.TryGetValue(sourceCategory.ParentId.Value, out var targetParentId)
					? targetParentId
					: null;

			categoryIdMap[sourceCategory.Id] = targetCategory.Id;

			if (targetCategory.Products.Count == 0 && sourceCategory.Products.Count > 0)
			{
				foreach (var sourceProduct in sourceCategory.Products.OrderBy(p => p.SortOrder))
				{
					await repoProduct.AddAsync(new Product
					{
						Id = Guid.NewGuid(),
						CategoryId = targetCategory.Id,
						Title = sourceProduct.Title,
						Slug = sourceProduct.Slug,
						Description = sourceProduct.Description,
						BasePrice = sourceProduct.BasePrice,
						SortOrder = sourceProduct.SortOrder,
						IsActive = sourceProduct.IsActive,
						Allergens = sourceProduct.Allergens,
						IsVegan = sourceProduct.IsVegan,
						IsVegetarian = sourceProduct.IsVegetarian,
						EstimatedPreparationTimeInMinutes = sourceProduct.EstimatedPreparationTimeInMinutes
					}, ct);
				}
			}
		}

		return mapper.Map<MenuDTO>(target);
	}
}
