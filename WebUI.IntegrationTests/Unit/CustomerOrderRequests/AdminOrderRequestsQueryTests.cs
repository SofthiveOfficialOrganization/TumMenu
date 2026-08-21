using Application.Abstractions;
using Application.Common.Exceptions;
using Application.CustomerOrderRequests.Commands;
using Application.CustomerOrderRequests.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.CustomerOrderRequests;

public class AdminOrderRequestsQueryTests
{
    private readonly ApplicationDbContext _db;

    public AdminOrderRequestsQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    [Fact]
    public async Task Handle_OwnerUser_ReturnsOnlyOwnCompanyOrders()
    {
        var ownerUserId = "owner-1";
        var ownCompany = await SeedCompanyAsync("Own", "own", ownerUserId);
        var otherCompany = await SeedCompanyAsync("Other", "other", "owner-2");
        var ownOrder = await SeedOrderAsync(ownCompany);
        await SeedOrderAsync(otherCompany);

        var handler = new GetAdminOrderRequestsQueryHandler(_db, new TestUserContext(ownerUserId));

        var result = await handler.Handle(new GetAdminOrderRequestsQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(ownOrder.Id);
    }

    [Fact]
    public async Task MarkSeen_OwnerUser_UpdatesOwnCompanyOrder()
    {
        var ownerUserId = "owner-1";
        var company = await SeedCompanyAsync("Own", "own", ownerUserId);
        var order = await SeedOrderAsync(company);
        var handler = new MarkOrderRequestSeenCommandHandler(_db, new TestUserContext(ownerUserId));

        await handler.Handle(new MarkOrderRequestSeenCommand(order.Id), CancellationToken.None);

        var saved = await _db.CustomerOrderRequests.SingleAsync(x => x.Id == order.Id);
        saved.Status.Should().Be(CustomerOrderRequestStatus.Seen);
    }

    [Fact]
    public async Task MarkSeen_OwnerUser_ForOtherCompanyOrderThrowsForbiddenAppException()
    {
        var company = await SeedCompanyAsync("Other", "other", "owner-2");
        var order = await SeedOrderAsync(company);
        var handler = new MarkOrderRequestSeenCommandHandler(_db, new TestUserContext("owner-1"));

        var act = () => handler.Handle(new MarkOrderRequestSeenCommand(order.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAppException>();
    }

    private async Task<Company> SeedCompanyAsync(string title, string slug, string ownerUserId)
    {
        var user = new ApplicationUser
        {
            Id = ownerUserId,
            UserName = $"{slug}@test.local",
            Email = $"{slug}@test.local"
        };
        var owner = new Owner
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            ApplicationUser = user
        };
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            OwnerId = owner.Id,
            Owner = owner
        };

        await _db.ApplicationUsers.AddAsync(user);
        await _db.Owners.AddAsync(owner);
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();
        return company;
    }

    private async Task<CustomerOrderRequest> SeedOrderAsync(Company company)
    {
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Title = $"{company.Title} Store",
            Slug = $"{company.Slug}-store",
            PhoneNumber = "000",
            CompanyId = company.Id
        };
        var qrCode = new QRCode
        {
            Id = Guid.NewGuid(),
            PublicKey = $"{company.Slug}-qr",
            StoreId = store.Id,
            IsActive = true
        };
        var session = new QrOrderSession
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            CompanyId = company.Id,
            QRCodeId = qrCode.Id,
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        var order = new CustomerOrderRequest
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            StoreId = store.Id,
            QrOrderSessionId = session.Id,
            CustomerName = "Ali",
            TableNumber = "3",
            Status = CustomerOrderRequestStatus.New,
            ItemCount = 1,
            Subtotal = 10m
        };
        order.Created();

        await _db.Stores.AddAsync(store);
        await _db.QRCodes.AddAsync(qrCode);
        await _db.QrOrderSessions.AddAsync(session);
        await _db.CustomerOrderRequests.AddAsync(order);
        await _db.SaveChangesAsync();
        return order;
    }

    private sealed class TestUserContext(string userId, bool isAdmin = false) : IUserContext
    {
        public bool IsAuthenticated => true;
        public string? UserId => userId;
        public string? UserName => userId;
        public string? Email => null;
        public string? OwnerId => null;
        public string? CompanyId => null;
        public string? CompanyName => null;
        public IReadOnlyList<string> Roles => isAdmin ? ["Admin"] : ["Owner"];
        public string? RemoteIp => "127.0.0.1";
        public bool IsAdmin => isAdmin;
        public Guid? CompanyIdParsed => null;
        public Guid? OwnerIdParsed => null;
    }
}
