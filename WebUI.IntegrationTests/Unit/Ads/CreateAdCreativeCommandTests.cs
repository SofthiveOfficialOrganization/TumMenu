using Application.Ads.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Ads;

public class CreateAdCreativeCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreateAdCreativeCommandTests()
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
    public async Task Handle_ValidCommand_CreativePersisted()
    {
        var handler = new CreateAdCreativeCommandHandler(Repo<AdCreative>(), _mapper);
        var command = new CreateAdCreativeCommand { Type = "image", Content = "https://example.com/ad.png" };

        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        result.Should().NotBeNull();
        var count = await _db.Set<AdCreative>().CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsTypeAndContent()
    {
        var handler = new CreateAdCreativeCommandHandler(Repo<AdCreative>(), _mapper);
        var command = new CreateAdCreativeCommand { Type = "html", Content = "<div>Ad</div>", ClickUrl = "https://click.example.com" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Type.Should().Be("html");
        result.Content.Should().Be("<div>Ad</div>");
        result.ClickUrl.Should().Be("https://click.example.com");
    }
}
