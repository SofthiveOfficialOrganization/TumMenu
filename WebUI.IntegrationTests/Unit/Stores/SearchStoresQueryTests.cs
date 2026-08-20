using Application.Stores.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Stores;

public class SearchStoresQueryTests
{
    private readonly ApplicationDbContext _db;

    public SearchStoresQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    private SearchStoresQueryHandler Handler() => new(Repo<Store>(), Repo<Product>());

    [Fact]
    public async Task Handle_WithoutLocation_ReturnsNewestStores()
    {
        var older = AddStore("Older", "older", 39.0, 35.0, DateTimeOffset.UtcNow.AddDays(-2));
        var newer = AddStore("Newer", "newer", 40.0, 36.0, DateTimeOffset.UtcNow.AddDays(-1));
        await _db.SaveChangesAsync();

        var result = await Handler().Handle(new SearchStoresQuery { Page = 1, PageSize = 10 }, CancellationToken.None);

        result.IsFallback.Should().BeFalse();
        result.Items.Select(i => i.Id).Should().Equal(newer.Id, older.Id);
        result.Items.Should().OnlyContain(i => i.DistanceKm == null);
    }

    [Fact]
    public async Task Handle_WhenNearbyStoreExists_DoesNotFallback()
    {
        var nearby = AddStore("Nearby", "nearby", 39.0005, 35.0005, DateTimeOffset.UtcNow.AddDays(-2));
        AddStore("Far", "far", 41.0, 39.0, DateTimeOffset.UtcNow.AddDays(-1));
        await _db.SaveChangesAsync();

        var result = await Handler().Handle(new SearchStoresQuery
        {
            UserLatitude = 39.0,
            UserLongitude = 35.0,
            MaxDistanceKm = 1,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.IsFallback.Should().BeFalse();
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(nearby.Id);
        result.Items[0].DistanceKm.Should().BeLessThan(1);
    }

    [Fact]
    public async Task Handle_WhenDistanceFilterFindsNothing_FallsBackToNewestFilteredStores()
    {
        var older = AddStore("Older", "older", 39.0, 35.0, DateTimeOffset.UtcNow.AddDays(-3));
        var newer = AddStore("Newer", "newer", 40.0, 36.0, DateTimeOffset.UtcNow.AddDays(-1));
        await _db.SaveChangesAsync();

        var result = await Handler().Handle(new SearchStoresQuery
        {
            UserLatitude = 10.0,
            UserLongitude = 10.0,
            MaxDistanceKm = 1,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.IsFallback.Should().BeTrue();
        result.FallbackMessage.Should().NotBeNullOrWhiteSpace();
        result.Items.Select(i => i.Id).Should().Equal(newer.Id, older.Id);
        result.Items.Should().OnlyContain(i => i.DistanceKm.HasValue);
    }

    [Fact]
    public async Task Handle_FallbackKeepsSearchCategoryAndVeganFilters()
    {
        var veganCategory = new CategoryLibraryItem { Id = Guid.NewGuid(), Title = "Salata", Slug = "salata" };
        var dessertCategory = new CategoryLibraryItem { Id = Guid.NewGuid(), Title = "Tatli", Slug = "tatli" };
        _db.CategoryLibraryItems.AddRange(veganCategory, dessertCategory);

        var veganStore = AddStore(
            "Vegan Place",
            "vegan-place",
            39.0,
            35.0,
            DateTimeOffset.UtcNow.AddDays(-2),
            veganCategory,
            "Avokado Salata",
            isVegan: true);

        AddStore(
            "Dessert Place",
            "dessert-place",
            40.0,
            36.0,
            DateTimeOffset.UtcNow.AddDays(-1),
            dessertCategory,
            "Sufle",
            isVegan: false);

        await _db.SaveChangesAsync();

        var result = await Handler().Handle(new SearchStoresQuery
        {
            SearchTerm = "salata",
            CategoryLibraryItemIds = [veganCategory.Id],
            IsVegan = true,
            UserLatitude = 10.0,
            UserLongitude = 10.0,
            MaxDistanceKm = 1,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.IsFallback.Should().BeTrue();
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(veganStore.Id);
    }

    [Fact]
    public async Task Handle_InvalidCoordinatesAreExcludedFromDistanceResults()
    {
        AddStore("Invalid", "invalid", 120.0, 35.0, DateTimeOffset.UtcNow.AddDays(-1));
        var valid = AddStore("Valid", "valid", 39.0, 35.0, DateTimeOffset.UtcNow.AddDays(-2));
        await _db.SaveChangesAsync();

        var result = await Handler().Handle(new SearchStoresQuery
        {
            UserLatitude = 39.0,
            UserLongitude = 35.0,
            MaxDistanceKm = 10,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.IsFallback.Should().BeFalse();
        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(valid.Id);
    }

    private Store AddStore(
        string title,
        string slug,
        double latitude,
        double longitude,
        DateTimeOffset createdAt,
        CategoryLibraryItem? categoryLibraryItem = null,
        string productTitle = "Mercimek Corbasi",
        bool isVegan = false)
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Title = $"{title} Company",
            Slug = $"{slug}-company",
            CreatedAt = createdAt
        };

        categoryLibraryItem ??= new CategoryLibraryItem
        {
            Id = Guid.NewGuid(),
            Title = "Corba",
            Slug = $"corba-{slug}",
            CreatedAt = createdAt
        };

        var store = new Store
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            PhoneNumber = "05001234567",
            Company = company,
            CompanyId = company.Id,
            ShowInSearchAndListings = true,
            CreatedAt = createdAt,
            Address = new Address
            {
                Id = Guid.NewGuid(),
                Latitude = latitude,
                Longitude = longitude,
                FullAddress = "Merkez, Test",
                CreatedAt = createdAt
            }
        };

        var menu = new Menu
        {
            Id = Guid.NewGuid(),
            Title = $"{title} Menu",
            Store = store,
            StoreId = store.Id,
            Status = MenuStatus.Active,
            CreatedAt = createdAt
        };

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Menu = menu,
            MenuId = menu.Id,
            CategoryLibraryItem = categoryLibraryItem,
            CategoryLibraryItemId = categoryLibraryItem.Id,
            IsActive = true,
            CreatedAt = createdAt
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Category = category,
            CategoryId = category.Id,
            Title = productTitle,
            Slug = productTitle.ToLowerInvariant().Replace(' ', '-'),
            BasePrice = 100,
            IsActive = true,
            IsVegan = isVegan,
            CreatedAt = createdAt
        };

        category.Products.Add(product);
        menu.Categories.Add(category);
        store.Menus.Add(menu);

        _db.Companies.Add(company);
        if (_db.Entry(categoryLibraryItem).State == EntityState.Detached)
        {
            _db.CategoryLibraryItems.Add(categoryLibraryItem);
        }
        _db.Stores.Add(store);

        return store;
    }
}
