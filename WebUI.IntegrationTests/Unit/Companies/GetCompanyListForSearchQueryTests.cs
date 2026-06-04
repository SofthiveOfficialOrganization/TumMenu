using Application.Abstractions;
using Application.Companies.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace WebUI.IntegrationTests.Unit.Companies;

public class GetCompanyListForSearchQueryTests
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IUserContext> _userContext;

    public GetCompanyListForSearchQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(opts);

        _userContext = new Mock<IUserContext>();
        _userContext.Setup(x => x.IsAdmin).Returns(true);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_WhenPageIsZero_NormalizesToFirstPage()
    {
        await _db.Companies.AddRangeAsync(
            new Company { Title = "Tum Menu", Slug = "tum-menu" },
            new Company { Title = "Another Company", Slug = "another-company" });
        await _db.SaveChangesAsync();

        var handler = new GetCompanyListForSearchHandler(Repo<Company>(), _userContext.Object);

        var result = await handler.Handle(
            new GetCompanyListForSearchQuery { SearchTerm = "tu", Page = 0, PageSize = 10 },
            CancellationToken.None);

        result.Index.Should().Be(1);
        result.From.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Tum Menu");
    }

    [Fact]
    public async Task Handle_ExcludesSingleStoreCompaniesThatAlreadyHaveStore()
    {
        var singleStoreWithStore = new Company { Title = "Single Full", Slug = "single-full", IsSingleStore = true };
        var singleStoreWithoutStore = new Company { Title = "Single Empty", Slug = "single-empty", IsSingleStore = true };
        var multiStore = new Company { Title = "Multi Store", Slug = "multi-store", IsSingleStore = false };

        await _db.Companies.AddRangeAsync(singleStoreWithStore, singleStoreWithoutStore, multiStore);
        await _db.Stores.AddAsync(new Store
        {
            Title = "Existing Store",
            Slug = "existing-store",
            PhoneNumber = "05001234567",
            CompanyId = singleStoreWithStore.Id
        });
        await _db.SaveChangesAsync();

        var handler = new GetCompanyListForSearchHandler(Repo<Company>(), _userContext.Object);

        var result = await handler.Handle(
            new GetCompanyListForSearchQuery { Page = 1, PageSize = 10 },
            CancellationToken.None);

        result.Items.Select(x => x.Title).Should().BeEquivalentTo("Single Empty", "Multi Store");
    }
}
