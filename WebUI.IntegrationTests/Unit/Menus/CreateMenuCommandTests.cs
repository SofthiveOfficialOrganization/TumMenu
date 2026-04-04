using Application.Menus.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Menus;

public class CreateMenuCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreateMenuCommandTests()
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
    public async Task Handle_ValidCommand_MenuIsPersistedAndLinkedToStore()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var handler = new CreateMenuToStoreCommandHandler(Repo<Menu>(), _mapper);
        var command = new CreateMenuToStoreCommand
        {
            Title = "Lunch Menu",
            StoreId = storeId,
            Status = MenuStatus.Active
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Lunch Menu");

        var saved = await _db.Menus.FirstOrDefaultAsync(m => m.StoreId == storeId);
        saved.Should().NotBeNull();
        saved!.StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_FirstMenuWithInactiveStatus_IsForcedActive()
    {
        // Arrange — no existing menus for this store
        var storeId = Guid.NewGuid();
        var handler = new CreateMenuToStoreCommandHandler(Repo<Menu>(), _mapper);
        var command = new CreateMenuToStoreCommand
        {
            Title = "First Menu",
            StoreId = storeId,
            Status = MenuStatus.Inactive
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        var saved = await _db.Menus.FirstOrDefaultAsync(m => m.StoreId == storeId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(MenuStatus.Active);
    }

    [Fact]
    public async Task Handle_NewActiveMenu_DeactivatesExistingActiveMenus()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var existing = new Menu { Title = "Old Menu", StoreId = storeId, Status = MenuStatus.Active };
        await _db.Menus.AddAsync(existing);
        await _db.SaveChangesAsync();

        var handler = new CreateMenuToStoreCommandHandler(Repo<Menu>(), _mapper);
        var command = new CreateMenuToStoreCommand
        {
            Title = "New Menu",
            StoreId = storeId,
            Status = MenuStatus.Active
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var oldMenu = await _db.Menus.FindAsync(existing.Id);
        oldMenu!.Status.Should().Be(MenuStatus.Inactive);
    }
}
