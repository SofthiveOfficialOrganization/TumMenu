using Application.Menus.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Products;

public class GetProductBySlugQueryTests
{
    private readonly ApplicationDbContext _db;

    public GetProductBySlugQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ReturnsProductPriceOptions()
    {
        // Arrange
        var company = new Company { Title = "Tum Cafe", Slug = "tum-cafe" };
        var store = new Store { Title = "Merkez", Slug = "merkez", PhoneNumber = "555", Company = company };
        var menu = new Menu { Title = "Aktif Menü", Store = store, Status = MenuStatus.Active };
        var categoryLibraryItem = new CategoryLibraryItem { Title = "İçecekler", Slug = "icecekler" };
        var category = new Category
        {
            Menu = menu,
            CategoryLibraryItem = categoryLibraryItem,
            SortOrder = 1,
            IsActive = true
        };
        var product = new Product
        {
            Title = "Limonata",
            Slug = "limonata",
            BasePrice = 45m,
            Category = category,
            IsActive = true,
            SortOrder = 1,
            Prices =
            [
                new ProductPrice { Size = "Bardak", Price = 45m },
                new ProductPrice { Size = "Sürahi", Price = 140m }
            ]
        };

        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();

        var handler = new GetProductBySlugHandler(Repo<Store>(), Repo<Menu>(), Repo<Category>());

        // Act
        var result = await handler.Handle(
            new GetProductBySlugQuery("tum-cafe", "merkez", "icecekler", "limonata"),
            CancellationToken.None);

        // Assert
        result.BasePrice.Should().Be(45m);
        result.Prices.Should().HaveCount(2);
        result.Prices.Select(p => p.Size).Should().Contain(new[] { "Bardak", "Sürahi" });
        result.Prices.Select(p => p.Price).Should().Contain(new[] { 45m, 140m });
    }
}
