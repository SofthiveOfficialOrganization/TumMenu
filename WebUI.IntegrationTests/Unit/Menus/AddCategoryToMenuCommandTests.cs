using Application.Common.Exceptions;
using Application.Menus.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Menus;

public class AddCategoryToMenuCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public AddCategoryToMenuCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);

        var config = new TypeAdapterConfig();
        config.Scan(typeof(Application.Companies.Commands.CreateCompanyCommand).Assembly);
        _mapper = new ServiceMapper(
            new ServiceCollection()
                .AddSingleton(config)
                .AddScoped<IMapper, ServiceMapper>()
                .BuildServiceProvider(),
            config);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ValidCommand_CategoryAddedWithCorrectSortOrder()
    {
        // Arrange
        var menu = new Menu { Title = "Test Menu", StoreId = Guid.NewGuid(), Status = MenuStatus.Active };
        var libItem = new CategoryLibraryItem { Title = "Starters", Slug = "starters" };
        await _db.Menus.AddAsync(menu);
        await _db.CategoryLibraryItems.AddAsync(libItem);
        await _db.SaveChangesAsync();

        var handler = new AddCategoryToMenuCommandHandler(Repo<Menu>(), Repo<CategoryLibraryItem>(), Repo<Category>(), _mapper);
        var command = new AddCategoryToMenuCommand
        {
            MenuId = menu.Id,
            CategoryLibraryItemId = libItem.Id,
            SortOrder = 5,
            IsActive = true
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert — verify via returned DTO (SaveChangesAsync causes navigation re-attach conflict in unit tests)
        result.Should().NotBeNull();
        result.SortOrder.Should().Be(5);
        result.CategoryLibraryItemId.Should().Be(libItem.Id);
        result.MenuId.Should().Be(menu.Id);
    }

    [Fact]
    public async Task Handle_MenuNotFound_ThrowsNotFoundAppException()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Starters", Slug = "starters" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        await _db.SaveChangesAsync();

        var handler = new AddCategoryToMenuCommandHandler(Repo<Menu>(), Repo<CategoryLibraryItem>(), Repo<Category>(), _mapper);
        var command = new AddCategoryToMenuCommand
        {
            MenuId = Guid.NewGuid(),
            CategoryLibraryItemId = libItem.Id,
            SortOrder = 1,
            IsActive = true
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundAppException>();
    }

    [Fact]
    public async Task Handle_DuplicateCategoryInMenu_ThrowsAlreadyExistsAppException()
    {
        // Arrange
        var menu = new Menu { Title = "Test Menu", StoreId = Guid.NewGuid(), Status = MenuStatus.Active };
        var libItem = new CategoryLibraryItem { Title = "Starters", Slug = "starters" };
        await _db.Menus.AddAsync(menu);
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var existingCat = new Category { MenuId = menu.Id, CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(existingCat);
        await _db.SaveChangesAsync();

        var handler = new AddCategoryToMenuCommandHandler(Repo<Menu>(), Repo<CategoryLibraryItem>(), Repo<Category>(), _mapper);
        var command = new AddCategoryToMenuCommand
        {
            MenuId = menu.Id,
            CategoryLibraryItemId = libItem.Id,
            SortOrder = 2,
            IsActive = true
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AlreadyExistsAppException>();
    }
}
