using Application.Stores.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Stores;

public class CheckStoreSlugQueryTests
{
    private readonly ApplicationDbContext _db;

    public CheckStoreSlugQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_SameSlugInDifferentCompany_IsAvailable()
    {
        var company1 = new Company { Title = "Company 1", Slug = "company-one" };
        var company2 = new Company { Title = "Company 2", Slug = "company-two" };
        await _db.Companies.AddRangeAsync(company1, company2);
        await _db.Stores.AddAsync(new Store
        {
            Title = "Main Store",
            Slug = "main-store",
            PhoneNumber = "05001234567",
            CompanyId = company1.Id
        });
        await _db.SaveChangesAsync();

        var handler = new CheckStoreSlugHandler(Repo<Store>());
        var result = await handler.Handle(new CheckStoreSlugQuery
        {
            Slug = "main-store",
            CompanyId = company2.Id
        }, CancellationToken.None);

        result.Available.Should().BeTrue();
        result.Message.Should().Be("Bu slug kullanılabilir.");
    }

    [Fact]
    public async Task Handle_SameSlugInSameCompany_IsUnavailable()
    {
        var company = new Company { Title = "Company", Slug = "company" };
        await _db.Companies.AddAsync(company);
        await _db.Stores.AddAsync(new Store
        {
            Title = "Main Store",
            Slug = "main-store",
            PhoneNumber = "05001234567",
            CompanyId = company.Id
        });
        await _db.SaveChangesAsync();

        var handler = new CheckStoreSlugHandler(Repo<Store>());
        var result = await handler.Handle(new CheckStoreSlugQuery
        {
            Slug = "main-store",
            CompanyId = company.Id
        }, CancellationToken.None);

        result.Available.Should().BeFalse();
        result.Message.Should().Be("Bu slug zaten kullanılmakta.");
    }
}
