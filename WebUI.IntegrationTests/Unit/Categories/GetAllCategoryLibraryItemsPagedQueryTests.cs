using Application.Categories.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Categories;

public class GetAllCategoryLibraryItemsPagedQueryTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetAllCategoryLibraryItemsPagedQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(opts);

        var config = new TypeAdapterConfig();
        config.Scan(typeof(GetAllCategoryLibraryItemsPagedQuery).Assembly);
        _mapper = new ServiceMapper(
            new ServiceCollection()
                .AddSingleton(config)
                .AddScoped<IMapper, ServiceMapper>()
                .BuildServiceProvider(),
            config);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_WithSearch_FiltersByDescription()
    {
        await _db.CategoryLibraryItems.AddRangeAsync(
            new CategoryLibraryItem { Title = "Ana Yemek", Slug = "ana-yemek", Description = "Sicak tabaklar" },
            new CategoryLibraryItem { Title = "Tatli", Slug = "tatli", Description = "Kapanis lezzetleri" });
        await _db.SaveChangesAsync();

        var handler = new GetAllCategoriesPagedHandler(Repo<CategoryLibraryItem>(), _mapper);

        var result = await handler.Handle(
            new GetAllCategoryLibraryItemsPagedQuery { Search = "tabak", Page = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Ana Yemek");
    }

    [Fact]
    public async Task Handle_WithTurkishDotlessISearch_MatchesAsciiI()
    {
        await _db.CategoryLibraryItems.AddRangeAsync(
            new CategoryLibraryItem { Title = "Izgara", Slug = "izgara" },
            new CategoryLibraryItem { Title = "Salata", Slug = "salata" });
        await _db.SaveChangesAsync();

        var handler = new GetAllCategoriesPagedHandler(Repo<CategoryLibraryItem>(), _mapper);

        var result = await handler.Handle(
            new GetAllCategoryLibraryItemsPagedQuery { Search = "ızgara", Page = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Izgara");
    }
}
