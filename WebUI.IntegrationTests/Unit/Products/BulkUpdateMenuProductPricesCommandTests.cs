using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Products.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Products;

public class BulkUpdateMenuProductPricesCommandTests
{
    private readonly ApplicationDbContext _db;

    public BulkUpdateMenuProductPricesCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_PercentageIncreaseWithNearest5_UpdatesBaseAndOptionPrices()
    {
        var menu = await SeedMenuAsync("owner-user");
        var category = await SeedCategoryAsync(menu.Id);
        var product = new Product
        {
            Title = "Burger",
            Slug = "burger",
            BasePrice = 255m,
            CategoryId = category.Id,
            IsActive = true,
            Prices =
            [
                new ProductPrice { Size = "Küçük", Price = 200m },
                new ProductPrice { Size = "Büyük", Price = 300m }
            ]
        };
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();

        var handler = new BulkUpdateMenuProductPricesCommandHandler(
            Repo<Menu>(),
            Repo<Product>(),
            new TestUserContext("owner-user"));

        var result = await handler.Handle(new BulkUpdateMenuProductPricesCommand
        {
            MenuId = menu.Id,
            UpdateType = BulkPriceUpdateType.Percentage,
            Amount = 10m,
            RoundingStrategy = BulkPriceRoundingStrategy.Nearest5
        }, CancellationToken.None);
        await _db.SaveChangesAsync();

        result.ProductCount.Should().Be(1);
        result.PriceCount.Should().Be(3);

        var saved = await _db.Products.Include(p => p.Prices).FirstAsync(p => p.Id == product.Id);
        saved.BasePrice.Should().Be(280m);
        saved.Prices.Select(p => p.Price).Should().BeEquivalentTo([220m, 330m]);
    }

    [Fact]
    public async Task Handle_FixedIncrease_AddsSameAmountToAllPrices()
    {
        var menu = await SeedMenuAsync("owner-user");
        var category = await SeedCategoryAsync(menu.Id);
        var product = new Product
        {
            Title = "Limonata",
            Slug = "limonata",
            BasePrice = 45m,
            CategoryId = category.Id,
            IsActive = true,
            Prices = [new ProductPrice { Size = "Büyük", Price = 60m }]
        };
        await _db.Products.AddAsync(product);
        await _db.SaveChangesAsync();

        var handler = new BulkUpdateMenuProductPricesCommandHandler(
            Repo<Menu>(),
            Repo<Product>(),
            new TestUserContext("owner-user"));

        await handler.Handle(new BulkUpdateMenuProductPricesCommand
        {
            MenuId = menu.Id,
            UpdateType = BulkPriceUpdateType.FixedAmount,
            Amount = 7.5m
        }, CancellationToken.None);
        await _db.SaveChangesAsync();

        var saved = await _db.Products.Include(p => p.Prices).FirstAsync(p => p.Id == product.Id);
        saved.BasePrice.Should().Be(52.5m);
        saved.Prices.Single().Price.Should().Be(67.5m);
    }

    [Fact]
    public async Task Handle_DifferentOwner_ThrowsForbidden()
    {
        var menu = await SeedMenuAsync("owner-user");
        await SeedCategoryAsync(menu.Id);
        await _db.SaveChangesAsync();

        var handler = new BulkUpdateMenuProductPricesCommandHandler(
            Repo<Menu>(),
            Repo<Product>(),
            new TestUserContext("another-owner"));

        var act = () => handler.Handle(new BulkUpdateMenuProductPricesCommand
        {
            MenuId = menu.Id,
            UpdateType = BulkPriceUpdateType.Percentage,
            Amount = 10m
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAppException>();
    }

    private async Task<Menu> SeedMenuAsync(string ownerUserId)
    {
        var user = new ApplicationUser
        {
            Id = ownerUserId,
            UserName = $"{ownerUserId}@test.local",
            Email = $"{ownerUserId}@test.local"
        };
        var owner = new Owner
        {
            ApplicationUserId = user.Id,
            ApplicationUser = user
        };
        var company = new Company
        {
            Title = "Test Sirket",
            Slug = "test-sirket",
            Owner = owner
        };
        var menu = new Menu
        {
            Title = "Ana Menu",
            Company = company,
            Status = MenuStatus.MainMenu
        };

        await _db.ApplicationUsers.AddAsync(user);
        await _db.Owners.AddAsync(owner);
        await _db.Companies.AddAsync(company);
        await _db.Menus.AddAsync(menu);
        await _db.SaveChangesAsync();
        return menu;
    }

    private async Task<Category> SeedCategoryAsync(Guid menuId)
    {
        var libItem = new CategoryLibraryItem { Title = "Yiyecekler", Slug = "yiyecekler" };
        await _db.CategoryLibraryItems.AddAsync(libItem);

        var category = new Category
        {
            MenuId = menuId,
            CategoryLibraryItem = libItem,
            SortOrder = 1,
            IsActive = true
        };
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();
        return category;
    }

    private sealed class TestUserContext(string userId, bool isAdmin = false) : IUserContext
    {
        public bool IsAuthenticated => true;
        public string? UserId => userId;
        public string? UserName => userId;
        public string? Email => $"{userId}@test.local";
        public string? OwnerId => null;
        public string? CompanyId => null;
        public string? CompanyName => null;
        public IReadOnlyList<string> Roles => isAdmin ? ["Admin"] : ["Owner"];
        public string? RemoteIp => null;
        public bool IsAdmin => isAdmin;
        public Guid? CompanyIdParsed => null;
        public Guid? OwnerIdParsed => null;
    }
}
