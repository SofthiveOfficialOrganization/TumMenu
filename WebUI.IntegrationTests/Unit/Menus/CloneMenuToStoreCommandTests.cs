using Application.Menus.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace WebUI.IntegrationTests.Unit.Menus;

public class CloneMenuToStoreCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CloneMenuToStoreCommandTests()
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
    public async Task Handle_ValidCommand_AppendsStoreNameAndDateToMenuTitle()
    {
        // Arrange
        var sourceMenu = new Menu
        {
            Title = "Ana Menü",
            CompanyId = Guid.NewGuid(),
            Status = MenuStatus.MainMenu
        };
        var store = new Store
        {
            Title = "Kadikoy",
            Slug = "kadikoy",
            PhoneNumber = "5550000000",
            CompanyId = Guid.NewGuid()
        };

        await _db.Menus.AddAsync(sourceMenu);
        await _db.Stores.AddAsync(store);
        await _db.SaveChangesAsync();

        var handler = new CloneMenuToStoreCommandHandler(
            Repo<Menu>(),
            Repo<Category>(),
            Repo<Product>(),
            Repo<Store>(),
            _mapper);

        var command = new CloneMenuToStoreCommand
        {
            SourceMenuId = sourceMenu.Id,
            StoreId = store.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        var resultTitle = result!.Title;
        resultTitle.Should().StartWith("Ana Menü - Kadikoy ");

        var datePart = resultTitle!.Replace("Ana Menü - Kadikoy ", string.Empty);
        DateTime.TryParseExact(datePart, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Handle_StoreNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sourceMenu = new Menu
        {
            Title = "Ana Menü",
            CompanyId = Guid.NewGuid(),
            Status = MenuStatus.MainMenu
        };

        await _db.Menus.AddAsync(sourceMenu);
        await _db.SaveChangesAsync();

        var handler = new CloneMenuToStoreCommandHandler(
            Repo<Menu>(),
            Repo<Category>(),
            Repo<Product>(),
            Repo<Store>(),
            _mapper);

        var command = new CloneMenuToStoreCommand
        {
            SourceMenuId = sourceMenu.Id,
            StoreId = Guid.NewGuid()
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Dükkan bulunamadı.");
    }
}
