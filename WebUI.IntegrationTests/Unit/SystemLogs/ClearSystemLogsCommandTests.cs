using Application.SystemLogs.Commands;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.SystemLogs;

public class ClearSystemLogsCommandTests
{
    private readonly ApplicationDbContext _db;
    private readonly ClearSystemLogsCommandHandler _handler;

    public ClearSystemLogsCommandTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(opts);
        _handler = new ClearSystemLogsCommandHandler(_db);
    }

    [Fact]
    public async Task Handle_DeletesOnlyLogsMatchingDateRangeAndStatusCodes()
    {
        await AddLog("old", new DateTimeOffset(2026, 5, 15, 10, 0, 0, TimeSpan.FromHours(3)), 500);
        await AddLog("match-500", new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.FromHours(3)), 500);
        await AddLog("match-200", new DateTimeOffset(2026, 5, 16, 11, 0, 0, TimeSpan.FromHours(3)), 200);
        await AddLog("kept-404", new DateTimeOffset(2026, 5, 16, 12, 0, 0, TimeSpan.FromHours(3)), 404);

        var deletedCount = await _handler.Handle(new ClearSystemLogsCommand
        {
            DateFrom = new DateOnly(2026, 5, 16),
            DateTo = new DateOnly(2026, 5, 16),
            StatusCodes = [200, 500]
        }, CancellationToken.None);

        deletedCount.Should().Be(2);
        var remainingTraceIds = await _db.SystemLogs
            .OrderBy(x => x.TraceId)
            .Select(x => x.TraceId)
            .ToListAsync();
        remainingTraceIds.Should().Equal("kept-404", "old");
    }

    [Fact]
    public async Task Handle_WhenStatusCodesAreEmpty_DeletesAllLogsInDateRange()
    {
        await AddLog("match-500", new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.FromHours(3)), 500);
        await AddLog("match-404", new DateTimeOffset(2026, 5, 16, 11, 0, 0, TimeSpan.FromHours(3)), 404);
        await AddLog("kept", new DateTimeOffset(2026, 5, 17, 10, 0, 0, TimeSpan.FromHours(3)), 500);

        var deletedCount = await _handler.Handle(new ClearSystemLogsCommand
        {
            DateFrom = new DateOnly(2026, 5, 16),
            DateTo = new DateOnly(2026, 5, 16)
        }, CancellationToken.None);

        deletedCount.Should().Be(2);
        _db.SystemLogs.Should().ContainSingle(x => x.TraceId == "kept");
    }

    private async Task AddLog(string traceId, DateTimeOffset createdAt, int statusCode)
    {
        await _db.SystemLogs.AddAsync(new SystemLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = createdAt,
            CreatedBy = "test",
            Level = statusCode >= 500 ? "Error" : "Warning",
            Source = "Test",
            StatusCode = statusCode,
            ResponseMessage = "Response message",
            TraceId = traceId,
            HttpMethod = "GET",
            Path = "/test",
            UserName = "test-user"
        });
        await _db.SaveChangesAsync();
    }
}
