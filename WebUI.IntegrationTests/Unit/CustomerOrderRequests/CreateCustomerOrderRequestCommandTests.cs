using Application.Common.Exceptions;
using Application.CustomerOrderRequests;
using Application.CustomerOrderRequests.Commands;
using Application.CustomerOrderRequests.DTOs;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.CustomerOrderRequests;

public class CreateCustomerOrderRequestCommandTests
{
    private readonly ApplicationDbContext _db;

    public CreateCustomerOrderRequestCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    [Fact]
    public async Task Handle_WithoutQrSession_ThrowsUnauthorizedAppException()
    {
        var handler = new CreateCustomerOrderRequestCommandHandler(_db);

        var act = () => handler.Handle(new CreateCustomerOrderRequestCommand
        {
            StoreId = Guid.NewGuid(),
            CustomerName = "Ali",
            TableNumber = "5",
            Items = [new CreateCustomerOrderRequestItemDTO { ProductId = Guid.NewGuid(), Quantity = 1 }]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WithValidQrSession_PersistsOrderWithServerSidePriceSnapshot()
    {
        var seed = await SeedMenuAsync();
        var token = await SeedSessionAsync(seed.Store, seed.QrCode, expiresAt: Now().AddMinutes(20));
        var handler = new CreateCustomerOrderRequestCommandHandler(_db);

        var result = await handler.Handle(new CreateCustomerOrderRequestCommand
        {
            SessionToken = token,
            StoreId = seed.Store.Id,
            CustomerName = "Ayşe",
            TableNumber = "12",
            Note = "Hızlı olursa sevinirim",
            Items =
            [
                new CreateCustomerOrderRequestItemDTO
                {
                    ProductId = seed.Product.Id,
                    ProductPriceId = seed.LargePrice.Id,
                    Quantity = 2,
                    Note = "Az pişmiş"
                }
            ]
        }, CancellationToken.None);

        result.Status.Should().Be(CustomerOrderRequestStatus.New);
        result.CustomerName.Should().Be("Ayşe");
        result.TableNumber.Should().Be("12");
        result.ItemCount.Should().Be(2);
        result.Subtotal.Should().Be(240m);
        result.Items.Should().ContainSingle();
        result.Items[0].ProductTitle.Should().Be("Kıymalı Pide");
        result.Items[0].ProductPriceSize.Should().Be("Büyük");
        result.Items[0].UnitPrice.Should().Be(120m);

        var saved = await _db.CustomerOrderRequests
            .Include(x => x.Items)
            .SingleAsync();
        saved.Subtotal.Should().Be(240m);
        saved.Items.Single().ProductTitleSnapshot.Should().Be("Kıymalı Pide");
    }

    [Fact]
    public async Task Handle_WithExpiredSession_ThrowsUnauthorizedAppException()
    {
        var seed = await SeedMenuAsync();
        var token = await SeedSessionAsync(seed.Store, seed.QrCode, expiresAt: Now().AddMinutes(-1));
        var handler = new CreateCustomerOrderRequestCommandHandler(_db);

        var act = () => handler.Handle(new CreateCustomerOrderRequestCommand
        {
            SessionToken = token,
            StoreId = seed.Store.Id,
            CustomerName = "Ali",
            TableNumber = "5",
            Items = [new CreateCustomerOrderRequestItemDTO { ProductId = seed.Product.Id, Quantity = 1 }]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }

    [Fact]
    public async Task Handle_WithDifferentStoreSession_ThrowsForbiddenAppException()
    {
        var seed = await SeedMenuAsync();
        var otherStore = new Store
        {
            Id = Guid.NewGuid(),
            Title = "Diğer Şube",
            Slug = "diger-sube",
            PhoneNumber = "000",
            CompanyId = seed.Company.Id
        };
        await _db.Stores.AddAsync(otherStore);
        await _db.SaveChangesAsync();
        var token = await SeedSessionAsync(otherStore, seed.QrCode, expiresAt: Now().AddMinutes(20));
        var handler = new CreateCustomerOrderRequestCommandHandler(_db);

        var act = () => handler.Handle(new CreateCustomerOrderRequestCommand
        {
            SessionToken = token,
            StoreId = seed.Store.Id,
            CustomerName = "Ali",
            TableNumber = "5",
            Items = [new CreateCustomerOrderRequestItemDTO { ProductId = seed.Product.Id, Quantity = 1 }]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAppException>();
    }

    [Fact]
    public async Task Handle_WithInactiveProduct_ThrowsUnprocessableAppException()
    {
        var seed = await SeedMenuAsync(productIsActive: false);
        var token = await SeedSessionAsync(seed.Store, seed.QrCode, expiresAt: Now().AddMinutes(20));
        var handler = new CreateCustomerOrderRequestCommandHandler(_db);

        var act = () => handler.Handle(new CreateCustomerOrderRequestCommand
        {
            SessionToken = token,
            StoreId = seed.Store.Id,
            CustomerName = "Ali",
            TableNumber = "5",
            Items = [new CreateCustomerOrderRequestItemDTO { ProductId = seed.Product.Id, Quantity = 1 }]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UnprocessableAppException>();
    }

    private async Task<string> SeedSessionAsync(Store store, QRCode qrCode, DateTimeOffset expiresAt)
    {
        var token = OrderSessionToken.Create();
        var session = new QrOrderSession
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            CompanyId = store.CompanyId,
            QRCodeId = qrCode.Id,
            TokenHash = OrderSessionToken.Hash(token),
            ExpiresAt = expiresAt,
            LastUsedAt = Now()
        };
        session.Created();
        await _db.QrOrderSessions.AddAsync(session);
        await _db.SaveChangesAsync();
        return token;
    }

    private async Task<MenuSeed> SeedMenuAsync(bool productIsActive = true)
    {
        var company = new Company { Id = Guid.NewGuid(), Title = "Pideci", Slug = "pideci" };
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Title = "Merkez",
            Slug = "merkez",
            PhoneNumber = "000",
            CompanyId = company.Id,
            Company = company
        };
        var qrCode = new QRCode
        {
            Id = Guid.NewGuid(),
            PublicKey = "qr-test",
            StoreId = store.Id,
            Store = store,
            IsActive = true
        };
        var menu = new Menu
        {
            Id = Guid.NewGuid(),
            Title = "Menü",
            StoreId = store.Id,
            Status = MenuStatus.Active
        };
        var libItem = new CategoryLibraryItem
        {
            Id = Guid.NewGuid(),
            Title = "Pideler",
            Slug = "pideler"
        };
        var category = new Category
        {
            Id = Guid.NewGuid(),
            MenuId = menu.Id,
            Menu = menu,
            CategoryLibraryItemId = libItem.Id,
            CategoryLibraryItem = libItem,
            IsActive = true
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category,
            Title = "Kıymalı Pide",
            Slug = "kiymali-pide",
            BasePrice = 90m,
            IsActive = productIsActive
        };
        var largePrice = new ProductPrice
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Size = "Büyük",
            Price = 120m
        };
        product.Prices.Add(largePrice);

        await _db.Companies.AddAsync(company);
        await _db.Stores.AddAsync(store);
        await _db.QRCodes.AddAsync(qrCode);
        await _db.Menus.AddAsync(menu);
        await _db.CategoryLibraryItems.AddAsync(libItem);
        await _db.Categories.AddAsync(category);
        await _db.Products.AddAsync(product);
        await _db.ProductPrices.AddAsync(largePrice);
        await _db.SaveChangesAsync();

        return new MenuSeed(company, store, qrCode, product, largePrice);
    }

    private static DateTimeOffset Now() => DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));

    private sealed record MenuSeed(
        Company Company,
        Store Store,
        QRCode QrCode,
        Product Product,
        ProductPrice LargePrice);
}
