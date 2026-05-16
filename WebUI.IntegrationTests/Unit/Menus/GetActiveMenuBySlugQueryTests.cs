using Application.Categories.DTOs;
using Application.Menus.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Menus;

public class GetActiveMenuBySlugQueryTests
{
	private readonly ApplicationDbContext _db;

	public GetActiveMenuBySlugQueryTests()
	{
		var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		_db = new ApplicationDbContext(opts);
	}

	private EfRepository<T> Repo<T>() where T : class => new(_db);

	[Fact]
	public async Task Handle_RootCategoryIncludesActiveSubCategoryProductsForMenuCounts()
	{
		var company = new Company
		{
			Id = Guid.NewGuid(),
			Title = "Test Company",
			Slug = "test-company"
		};
		var store = new Store
		{
			Id = Guid.NewGuid(),
			Title = "Test Store",
			Slug = "test-store",
			PhoneNumber = "05000000000",
			CompanyId = company.Id
		};
		var menu = new Menu
		{
			Id = Guid.NewGuid(),
			Title = "Menu",
			StoreId = store.Id,
			Status = MenuStatus.Active
		};
		var rootLibraryItem = new CategoryLibraryItem
		{
			Id = Guid.NewGuid(),
			Title = "X",
			Slug = "x"
		};
		var root = new Category
		{
			Id = Guid.NewGuid(),
			MenuId = menu.Id,
			CategoryLibraryItemId = rootLibraryItem.Id,
			SortOrder = 1,
			IsActive = true
		};

		await _db.Companies.AddAsync(company);
		await _db.Stores.AddAsync(store);
		await _db.Menus.AddAsync(menu);
		await _db.CategoryLibraryItems.AddAsync(rootLibraryItem);
		await _db.Categories.AddAsync(root);

		for (var categoryIndex = 0; categoryIndex < 3; categoryIndex++)
		{
			var libraryItem = new CategoryLibraryItem
			{
				Id = Guid.NewGuid(),
				Title = ((char)('A' + categoryIndex)).ToString(),
				Slug = ((char)('a' + categoryIndex)).ToString()
			};
			var subCategory = new Category
			{
				Id = Guid.NewGuid(),
				MenuId = menu.Id,
				CategoryLibraryItemId = libraryItem.Id,
				ParentId = root.Id,
				SortOrder = categoryIndex,
				IsActive = true
			};

			await _db.CategoryLibraryItems.AddAsync(libraryItem);
			await _db.Categories.AddAsync(subCategory);

			for (var productIndex = 0; productIndex < 3; productIndex++)
			{
				await _db.Products.AddAsync(new Product
				{
					Id = Guid.NewGuid(),
					CategoryId = subCategory.Id,
					Title = $"Product {categoryIndex}-{productIndex}",
					Slug = $"product-{categoryIndex}-{productIndex}",
					BasePrice = 10,
					SortOrder = productIndex,
					IsActive = true
				});
			}
		}

		await _db.SaveChangesAsync();

		var handler = new GetActiveMenuBySlugHandler(Repo<Store>(), Repo<Menu>());

		var result = await handler.Handle(new GetActiveMenuBySlugQuery(company.Slug, store.Slug), CancellationToken.None);

		var rootCategory = result.Categories.Should().ContainSingle().Subject;
		rootCategory.Products.Should().BeEmpty();
		rootCategory.SubCategories.Should().HaveCount(3);
		CountActiveProducts(rootCategory).Should().Be(9);
	}

	private static int CountActiveProducts(CategoryDTO category)
	{
		var directProductCount = category.Products.Count(p => p.IsActive);
		var subCategoryProductCount = category.SubCategories.Where(c => c.IsActive).Sum(CountActiveProducts);

		return directProductCount + subCategoryProductCount;
	}
}
