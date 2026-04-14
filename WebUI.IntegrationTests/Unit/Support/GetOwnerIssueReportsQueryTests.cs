using Application.Support.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Support;

public class GetOwnerIssueReportsQueryTests
{
    private readonly ApplicationDbContext _db;

    public GetOwnerIssueReportsQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_FilteredResult_KeepsGlobalStatusCounts()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "owner-user",
            UserName = "owner1",
            Email = "owner1@test.local"
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
        var resolvedReport = new OwnerIssueReport
        {
            Owner = owner,
            Description = "Resolved issue",
            CurrentPageTitle = "Dashboard",
            CurrentPageUrl = "/admin/dashboard",
            Status = IssueReportStatus.Resolved,
            CreatedAt = DateTimeOffset.UtcNow,
            ResolvedAt = DateTimeOffset.UtcNow
        };

        await _db.ApplicationUsers.AddAsync(user);
        await _db.Owners.AddAsync(owner);
        await _db.Companies.AddAsync(company);
        await _db.OwnerIssueReports.AddAsync(resolvedReport);
        await _db.SaveChangesAsync();

        var handler = new GetOwnerIssueReportsHandler(Repo<OwnerIssueReport>());

        // Act
        var result = await handler.Handle(new GetOwnerIssueReportsQuery(IssueReportStatus.Open), CancellationToken.None);

        // Assert
        result.Reports.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
        result.OpenCount.Should().Be(0);
        result.InProgressCount.Should().Be(0);
        result.ResolvedCount.Should().Be(1);
        result.ClosedCount.Should().Be(0);
    }
}
