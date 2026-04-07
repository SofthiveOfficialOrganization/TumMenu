using Application.Ads.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace WebUI.IntegrationTests.Unit.Ads;

public class ImportAdRevenueCommandTests
{
    private readonly ApplicationDbContext _db;

    public ImportAdRevenueCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    private static IFormFile MakeCsv(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "File", "report.csv")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }

    [Fact]
    public async Task Handle_ValidCsv_InsertsRows()
    {
        var csv = "Date,Impressions,Clicks,Revenue,Currency\n2026-01-01,1000,50,12.50,TRY\n2026-01-02,900,40,11.00,TRY";
        var handler = new ImportAdRevenueCommandHandler(Repo<AdRevenueImport>());

        var result = await handler.Handle(
            new ImportAdRevenueCommand { File = MakeCsv(csv) }, CancellationToken.None);

        result.ImportedCount.Should().Be(2);
        result.SkippedCount.Should().Be(0);
        var count = await _db.Set<AdRevenueImport>().CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_DuplicateDay_SkipsExistingRow()
    {
        await _db.Set<AdRevenueImport>().AddAsync(new AdRevenueImport
        {
            Day = new DateOnly(2026, 1, 1), Revenue = 10m, Network = "AdSense", Currency = "TRY"
        });
        await _db.SaveChangesAsync();

        var csv = "Date,Impressions,Clicks,Revenue,Currency\n2026-01-01,1000,50,12.50,TRY\n2026-01-02,900,40,11.00,TRY";
        var handler = new ImportAdRevenueCommandHandler(Repo<AdRevenueImport>());

        var result = await handler.Handle(
            new ImportAdRevenueCommand { File = MakeCsv(csv) }, CancellationToken.None);

        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MalformedRow_SkipsRow()
    {
        var csv = "Date,Impressions,Clicks,Revenue,Currency\nnot-a-date,1000,50,12.50,TRY\n2026-01-02,900,40,11.00,TRY";
        var handler = new ImportAdRevenueCommandHandler(Repo<AdRevenueImport>());

        var result = await handler.Handle(
            new ImportAdRevenueCommand { File = MakeCsv(csv) }, CancellationToken.None);

        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(1);
    }
}
