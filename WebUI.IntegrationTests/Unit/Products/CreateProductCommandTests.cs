using Application.Common.Exceptions;
using Application.Products.Commands;
using Application.Products.DTOs;
using Application.Products.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebUI.IntegrationTests.Unit.Products;

public class CreateProductCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CreateProductCommandTests()
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
    public async Task Handle_ValidCommand_ProductWithPriceIsPersisted()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Main", Slug = "main" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var category = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(Repo<Product>(), Repo<Category>(), _mapper);
        var command = new CreateProductCommand
        {
            Title = "Burger",
            BasePrice = 12.50m,
            IsActive = true,
            CategoryId = category.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Burger");
        result.BasePrice.Should().Be(12.50m);

        var saved = await _db.Products.FirstOrDefaultAsync(p => p.CategoryId == category.Id);
        saved.Should().NotBeNull();
        saved!.BasePrice.Should().Be(12.50m);
    }

    [Fact]
    public async Task Handle_WithPriceOptions_PersistsOnlyCompleteValidOptions()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Drinks", Slug = "drinks" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var category = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(Repo<Product>(), Repo<Category>(), _mapper);
        var command = new CreateProductCommand
        {
            Title = "Limonata",
            BasePrice = 40m,
            IsActive = true,
            CategoryId = category.Id,
            Prices =
            [
                new ProductPriceInputDTO { Size = "Küçük", Price = 35m },
                new ProductPriceInputDTO { Size = "  Büyük  ", Price = 55m },
                new ProductPriceInputDTO { Size = "", Price = 60m },
                new ProductPriceInputDTO { Size = "Geçersiz", Price = 10000m },
                new ProductPriceInputDTO { Size = "Fiyatsız", Price = null }
            ]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Prices.Should().HaveCount(2);
        result.Prices.Select(p => p.Size).Should().ContainInOrder("Küçük", "Büyük");

        var saved = await _db.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.CategoryId == category.Id);

        saved.Should().NotBeNull();
        saved!.Prices.Should().HaveCount(2);
        saved.Prices.Select(p => p.Size).Should().ContainInOrder("Küçük", "Büyük");
        saved.Prices.Select(p => p.Price).Should().ContainInOrder(35m, 55m);
    }

    [Fact]
    public async Task GetProductById_ReturnsPriceOptions()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Desserts", Slug = "desserts" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var category = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(category);
        var product = new Product
        {
            Title = "Cheesecake",
            Slug = "cheesecake",
            BasePrice = 95m,
            CategoryId = category.Id,
            IsActive = true,
            Prices =
            [
                new ProductPrice { Size = "Dilim", Price = 95m },
                new ProductPrice { Size = "Bütün", Price = 650m }
            ]
        };
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();

        var handler = new GetProductByIdHandler(Repo<Product>(), _mapper);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        // Assert
        result.Prices.Should().HaveCount(2);
        result.Prices.Select(p => p.Size).Should().Contain(new[] { "Dilim", "Bütün" });
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsUnprocessableAppException()
    {
        // Arrange — no category in DB
        var handler = new CreateProductCommandHandler(Repo<Product>(), Repo<Category>(), _mapper);
        var command = new CreateProductCommand
        {
            Title = "Pizza",
            BasePrice = 15.00m,
            IsActive = true,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableAppException>();
    }

    [Fact]
    public async Task Handle_NoSlugProvided_SlugIsAutoGenerated()
    {
        // Arrange
        var libItem = new CategoryLibraryItem { Title = "Main", Slug = "main" };
        await _db.CategoryLibraryItems.AddAsync(libItem);
        var category = new Category { MenuId = Guid.NewGuid(), CategoryLibraryItemId = libItem.Id, SortOrder = 1, IsActive = true };
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(Repo<Product>(), Repo<Category>(), _mapper);
        var command = new CreateProductCommand
        {
            Title = "Adana Kebap",
            BasePrice = 20.00m,
            IsActive = true,
            CategoryId = category.Id,
            Slug = null
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        var saved = await _db.Products.FirstOrDefaultAsync(p => p.CategoryId == category.Id);
        saved.Should().NotBeNull();
        saved!.Slug.Should().Be("adana-kebap");
    }
}
