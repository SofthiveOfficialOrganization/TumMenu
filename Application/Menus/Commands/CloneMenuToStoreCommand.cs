using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class CloneMenuToStoreCommand : IRequest<MenuDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Menü kopyalandı";
	public Guid SourceMenuId { get; set; }
	public Guid StoreId { get; set; }
}

public class CloneMenuToStoreCommandHandler(
	IRepository<Menu> repoMenu,
	IRepository<Category> repoCategory,
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<CloneMenuToStoreCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CloneMenuToStoreCommand req, CancellationToken ct)
	{
		var source = await repoMenu.Query()
			.AsSplitQuery()
			.Include(m => m.Categories)
				.ThenInclude(c => c.Products)
			.FirstOrDefaultAsync(m => m.Id == req.SourceMenuId, ct)
			?? throw new KeyNotFoundException("Kaynak menü bulunamadı.");

		// Deactivate other active menus for this store
		var existingActive = await repoMenu.Query(tracked: true)
			.Where(m => m.StoreId == req.StoreId && m.Status == MenuStatus.Active)
			.ToListAsync(ct);
		foreach (var m in existingActive) m.Status = MenuStatus.Inactive;

		var newMenu = new Menu
		{
			Title = source.Title,
			StoreId = req.StoreId,
			Status = MenuStatus.Active
		};
		await repoMenu.AddAsync(newMenu, ct);

		// Clone categories: roots first, then sub-categories (preserving hierarchy)
		var categoryIdMap = new Dictionary<Guid, Guid>();
		var allCategories = source.Categories
			.OrderBy(c => c.ParentId.HasValue ? 1 : 0)
			.ThenBy(c => c.SortOrder)
			.ToList();

		foreach (var srcCat in allCategories)
		{
			Guid? newParentId = srcCat.ParentId.HasValue && categoryIdMap.TryGetValue(srcCat.ParentId.Value, out var pid)
				? pid : null;

			var newCat = new Category
			{
				MenuId = newMenu.Id,
				CategoryLibraryItemId = srcCat.CategoryLibraryItemId,
				SortOrder = srcCat.SortOrder,
				IsActive = srcCat.IsActive,
				ParentId = newParentId
			};
			await repoCategory.AddAsync(newCat, ct);
			categoryIdMap[srcCat.Id] = newCat.Id;

			foreach (var srcProd in srcCat.Products.OrderBy(p => p.SortOrder))
			{
				await repoProduct.AddAsync(new Product
				{
					CategoryId = newCat.Id,
					Title = srcProd.Title,
					Slug = srcProd.Slug,
					Description = srcProd.Description,
					BasePrice = srcProd.BasePrice,
					SortOrder = srcProd.SortOrder,
					IsActive = srcProd.IsActive,
					Allergens = srcProd.Allergens,
					IsVegan = srcProd.IsVegan,
					IsVegetarian = srcProd.IsVegetarian,
					EstimatedPreparationTimeInMinutes = srcProd.EstimatedPreparationTimeInMinutes
				}, ct);
			}
		}

		return mapper.Map<MenuDTO>(newMenu);
	}
}
