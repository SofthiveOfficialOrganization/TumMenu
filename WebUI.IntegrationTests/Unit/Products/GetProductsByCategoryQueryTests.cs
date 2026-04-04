using Application.Products.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Products;

public class GetProductsByCategoryQueryTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProductsByCategoryQueryTests()
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
    public async Task Handle_ReturnsProductsFilteredByCategory()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Main", Slug = "main" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var catA = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        var catB = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddRangeAsync(catA, catB);

        var p1 = new Product { Title = "Burger", Slug = "burger", BasePrice = 10, CategoryId = catA.Id, SortOrder = 1 };
        var p2 = new Product { Title = "Pizza", Slug = "pizza", BasePrice = 12, CategoryId = catA.Id, SortOrder = 2 };
        var p3 = new Product { Title = "Salad", Slug = "salad", BasePrice = 8, CategoryId = catB.Id, SortOrder = 1 };
        await _db.Products.AddRangeAsync(p1, p2, p3);
        await _db.SaveChangesAsync();

        var handler = new GetProductsByCategoryIdHandler(Repo<Product>(), _mapper);
        var query = new GetProductsPagedByCategoryIdQuery { CategoryId = catA.Id, Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Select(p => p.Title).Should().Contain(new[] { "Burger", "Pizza" });
        result.Items.Select(p => p.Title).Should().NotContain("Salad");
    }

    [Fact]
    public async Task Handle_NoCategoryMatch_ReturnsEmptyList()
    {
        // Arrange — no products for this category
        var handler = new GetProductsByCategoryIdHandler(Repo<Product>(), _mapper);
        var query = new GetProductsPagedByCategoryIdQuery { CategoryId = Guid.NewGuid(), Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SearchFilter_ReturnsMatchingProducts()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Drinks", Slug = "drinks" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var cat = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(cat);

        var p1 = new Product { Title = "Cola", Slug = "cola", BasePrice = 3, CategoryId = cat.Id, SortOrder = 1 };
        var p2 = new Product { Title = "Lemonade", Slug = "lemonade", BasePrice = 4, CategoryId = cat.Id, SortOrder = 2 };
        await _db.Products.AddRangeAsync(p1, p2);
        await _db.SaveChangesAsync();

        var handler = new GetProductsByCategoryIdHandler(Repo<Product>(), _mapper);
        var query = new GetProductsPagedByCategoryIdQuery { CategoryId = cat.Id, Search = "Cola", Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Cola");
    }
}
