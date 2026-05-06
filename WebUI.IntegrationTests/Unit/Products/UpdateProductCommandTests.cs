using Application.Products.Commands;
using Application.Products.DTOs;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Products;

public class UpdateProductCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateProductCommandTests()
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
    public async Task Handle_ReplacesPriceOptions()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Main", Slug = "main" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var category = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(category);
        var product = new Product
        {
            Title = "Burger",
            Slug = "burger",
            BasePrice = 100m,
            CategoryId = category.Id,
            SortOrder = 1,
            IsActive = true,
            Prices =
            [
                new ProductPrice { Size = "Eski Küçük", Price = 90m },
                new ProductPrice { Size = "Eski Büyük", Price = 130m }
            ]
        };
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(Repo<Product>(), Repo<Tag>(), _mapper);
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Title = "Burger Menü",
            CategoryId = category.Id,
            BasePrice = 120m,
            SortOrder = 2,
            IsActive = true,
            Prices =
            [
                new ProductPriceInputDTO { Size = "Orta", Price = 120m },
                new ProductPriceInputDTO { Size = "  Büyük  ", Price = 150m },
                new ProductPriceInputDTO { Size = "", Price = 160m }
            ]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Prices.Should().HaveCount(2);
        result.Prices.Select(p => p.Size).Should().ContainInOrder("Orta", "Büyük");

        var saved = await _db.Products
            .Include(p => p.Prices)
            .FirstAsync(p => p.Id == product.Id);

        saved.Title.Should().Be("Burger Menü");
        saved.BasePrice.Should().Be(120m);
        saved.Prices.Should().HaveCount(2);
        saved.Prices.Select(p => p.Size).Should().ContainInOrder("Orta", "Büyük");
        saved.Prices.Select(p => p.Price).Should().ContainInOrder(120m, 150m);
    }
}
