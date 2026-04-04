using Application.Ads.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Ads;

public class CreateAdPlacementCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreateAdPlacementCommandTests()
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
    public async Task Handle_ValidCommand_PlacementPersisted()
    {
        var slotId = Guid.NewGuid();
        var creativeId = Guid.NewGuid();

        var handler = new CreateAdPlacementCommandHandler(Repo<AdPlacement>(), _mapper);
        var command = new CreateAdPlacementCommand
        {
            AdSlotId = slotId,
            AdCreativeId = creativeId,
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        result.Should().NotBeNull();
        var count = await _db.Set<AdPlacement>().CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_PlacementLinkedToSlotAndCreative()
    {
        var slotId = Guid.NewGuid();
        var creativeId = Guid.NewGuid();

        var handler = new CreateAdPlacementCommandHandler(Repo<AdPlacement>(), _mapper);
        var command = new CreateAdPlacementCommand
        {
            AdSlotId = slotId,
            AdCreativeId = creativeId,
            IsActive = true,
            DailyCap = 100
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.AdSlotId.Should().Be(slotId);
        result.AdCreativeId.Should().Be(creativeId);
        result.IsActive.Should().BeTrue();
        result.DailyCap.Should().Be(100);
    }
}
