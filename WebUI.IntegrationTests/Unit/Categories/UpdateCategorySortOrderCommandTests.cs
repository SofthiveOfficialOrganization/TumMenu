using Application.Categories.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Categories;

public class UpdateCategorySortOrderCommandTests
{
	private readonly ApplicationDbContext _db;

	public UpdateCategorySortOrderCommandTests()
	{
		var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		_db = new ApplicationDbContext(opts);
	}

	private EfRepository<T> Repo<T>() where T : class => new(_db);

	[Fact]
	public async Task Handle_UpdatesSortOrderAndVisibilityWhenStatusProvided()
	{
		var category = new Category
		{
			MenuId = Guid.NewGuid(),
			CategoryLibraryItemId = Guid.NewGuid(),
			SortOrder = 1,
			IsActive = true
		};
		await _db.Categories.AddAsync(category);
		await _db.SaveChangesAsync();

		var handler = new UpdateCategorySortOrderCommandHandler(Repo<Category>());

		await handler.Handle(new UpdateCategorySortOrderCommand
		{
			Items =
			[
				new CategorySortItem
				{
					Id = category.Id,
					SortOrder = 3,
					IsActive = false
				}
			]
		}, CancellationToken.None);
		await _db.SaveChangesAsync();

		var saved = await _db.Categories.FirstAsync(c => c.Id == category.Id);
		saved.SortOrder.Should().Be(3);
		saved.IsActive.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_LeavesVisibilityUnchangedWhenStatusNotProvided()
	{
		var category = new Category
		{
			MenuId = Guid.NewGuid(),
			CategoryLibraryItemId = Guid.NewGuid(),
			SortOrder = 1,
			IsActive = true
		};
		await _db.Categories.AddAsync(category);
		await _db.SaveChangesAsync();

		var handler = new UpdateCategorySortOrderCommandHandler(Repo<Category>());

		await handler.Handle(new UpdateCategorySortOrderCommand
		{
			Items = [new CategorySortItem { Id = category.Id, SortOrder = 2 }]
		}, CancellationToken.None);
		await _db.SaveChangesAsync();

		var saved = await _db.Categories.FirstAsync(c => c.Id == category.Id);
		saved.SortOrder.Should().Be(2);
		saved.IsActive.Should().BeTrue();
	}
}
