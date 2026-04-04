using Application.Ads.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Ads;

public class CreateAdSlotCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreateAdSlotCommandTests()
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
    public async Task Handle_ValidCommand_SlotPersisted()
    {
        var handler = new CreateAdSlotCommandHandler(Repo<AdSlot>(), _mapper);
        var command = new CreateAdSlotCommand { Key = "store_header", IsActive = true };

        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        result.Should().NotBeNull();
        var count = await _db.Set<AdSlot>().CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsKeyAndIsActive()
    {
        var handler = new CreateAdSlotCommandHandler(Repo<AdSlot>(), _mapper);
        var command = new CreateAdSlotCommand { Key = "menu_inline", IsActive = true, Description = "Test slot" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Key.Should().Be("menu_inline");
        result.IsActive.Should().BeTrue();
    }
}
