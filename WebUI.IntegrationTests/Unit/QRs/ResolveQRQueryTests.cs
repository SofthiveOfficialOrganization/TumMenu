using Application.Common.Exceptions;
using Application.QRs.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace WebUI.IntegrationTests.Unit.QRs;

public class ResolveQRQueryTests
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public ResolveQRQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);

        var cacheMock = new Mock<IMemoryCache>();
        object? outVal = null;
        cacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out outVal)).Returns(false);
        cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        _cache = cacheMock.Object;
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_ValidKey_ReturnsUrlWithIsQrParam()
    {
        // Arrange
        var company = new Company { Title = "Test Co", Slug = "test-co" };
        var store = new Store { Title = "Main Store", Slug = "main-store", PhoneNumber = "0000", Company = company };
        var qr = new QRCode
        {
            PublicKey = "abc1234",
            Store = store,
            StoreId = store.Id,
            ResolveMode = QRResolveMode.LatestActive,
            IsActive = true
        };
        await _db.Companies.AddAsync(company);
        await _db.Stores.AddAsync(store);
        await _db.Set<QRCode>().AddAsync(qr);
        await _db.SaveChangesAsync();

        var handler = new ResolveQRQueryHandler(Repo<QRCode>(), _cache);

        // Act
        var result = await handler.Handle(new ResolveQRQuery("abc1234"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Contain("isQr=true");
        result.QRCodeId.Should().Be(qr.Id);
    }

    [Fact]
    public async Task Handle_InvalidKey_ThrowsNotFoundAppException()
    {
        var handler = new ResolveQRQueryHandler(Repo<QRCode>(), _cache);

        var act = () => handler.Handle(new ResolveQRQuery("nonexistent"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }

    [Fact]
    public async Task Handle_InactiveQR_ThrowsNotFoundAppException()
    {
        var qr = new QRCode
        {
            PublicKey = "inactive1",
            ResolveMode = QRResolveMode.LatestActive,
            IsActive = false
        };
        await _db.Set<QRCode>().AddAsync(qr);
        await _db.SaveChangesAsync();

        var handler = new ResolveQRQueryHandler(Repo<QRCode>(), _cache);

        var act = () => handler.Handle(new ResolveQRQuery("inactive1"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
