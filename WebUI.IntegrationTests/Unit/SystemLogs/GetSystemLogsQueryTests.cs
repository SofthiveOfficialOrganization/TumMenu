using Application.SystemLogs.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.SystemLogs;

public class GetSystemLogsQueryTests
{
    private readonly ApplicationDbContext _db;
    private readonly GetSystemLogsQueryHandler _handler;

    public GetSystemLogsQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
        _handler = new GetSystemLogsQueryHandler(_db);
    }

    [Fact]
    public async Task Handle_FiltersByDateRange()
    {
        await AddLog("old", new DateTimeOffset(2026, 5, 15, 10, 0, 0, TimeSpan.FromHours(3)));
        await AddLog("match", new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.FromHours(3)));

        var result = await _handler.Handle(new GetSystemLogsQuery
        {
            DateFrom = new DateOnly(2026, 5, 16),
            DateTo = new DateOnly(2026, 5, 16)
        }, CancellationToken.None);

        result.Items.Should().ContainSingle(x => x.TraceId == "match");
    }

    [Fact]
    public async Task Handle_FiltersByTimeRange()
    {
        await AddLog("outside", new DateTimeOffset(2026, 5, 16, 9, 30, 0, TimeSpan.FromHours(3)));
        await AddLog("inside", new DateTimeOffset(2026, 5, 16, 10, 30, 0, TimeSpan.FromHours(3)));

        var result = await _handler.Handle(new GetSystemLogsQuery
        {
            DateFrom = new DateOnly(2026, 5, 16),
            DateTo = new DateOnly(2026, 5, 16),
            TimeFrom = new TimeOnly(10, 0),
            TimeTo = new TimeOnly(11, 0)
        }, CancellationToken.None);

        result.Items.Should().ContainSingle(x => x.TraceId == "inside");
    }

    [Fact]
    public async Task Handle_FiltersByStatusUserAndSearch()
    {
        await AddLog("miss", DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)), 404, "other-user", "Bulunamadı", "/missing");
        await AddLog("hit", DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)), 500, "admin-user", "Beklenmeyen hata", "/Admin/SystemLogs");

        var result = await _handler.Handle(new GetSystemLogsQuery
        {
            StatusCode = 500,
            User = "admin",
            Search = "SystemLogs"
        }, CancellationToken.None);

        result.Items.Should().ContainSingle(x => x.TraceId == "hit");
    }

    [Fact]
    public async Task Handle_ReturnsNewestFirstAndPaginates()
    {
        await AddLog("oldest", new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.FromHours(3)));
        await AddLog("middle", new DateTimeOffset(2026, 5, 16, 11, 0, 0, TimeSpan.FromHours(3)));
        await AddLog("newest", new DateTimeOffset(2026, 5, 16, 12, 0, 0, TimeSpan.FromHours(3)));

        var result = await _handler.Handle(new GetSystemLogsQuery
        {
            DateFrom = new DateOnly(2026, 5, 16),
            DateTo = new DateOnly(2026, 5, 16),
            PageSize = 2
        }, CancellationToken.None);

        result.Items.Select(x => x.TraceId).Should().Equal("newest", "middle");
        result.Count.Should().Be(3);
        result.HasNext.Should().BeTrue();
    }

    private async Task AddLog(
        string traceId,
        DateTimeOffset createdAt,
        int statusCode = 500,
        string userName = "test-user",
        string responseMessage = "Response message",
        string path = "/test")
    {
        await _db.SystemLogs.AddAsync(new SystemLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = createdAt,
            CreatedBy = "test",
            Level = statusCode >= 500 ? "Error" : "Warning",
            Source = "Test",
            StatusCode = statusCode,
            ResponseMessage = responseMessage,
            ExceptionType = "System.Exception",
            ExceptionMessage = responseMessage,
            StackTrace = $"Stack {responseMessage}",
            TraceId = traceId,
            HttpMethod = "GET",
            Path = path,
            UserId = userName,
            UserName = userName
        });
        await _db.SaveChangesAsync();
    }
}
