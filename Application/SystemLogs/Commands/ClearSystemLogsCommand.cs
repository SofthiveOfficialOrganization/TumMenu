using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.SystemLogs.Commands;

public sealed class ClearSystemLogsCommand : IRequest<int>
{
    public DateOnly DateFrom { get; init; }
    public DateOnly DateTo { get; init; }
    public int[] StatusCodes { get; init; } = [];
}

public sealed class ClearSystemLogsCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ClearSystemLogsCommand, int>
{
    public async Task<int> Handle(ClearSystemLogsCommand req, CancellationToken ct)
    {
        if (req.DateTo < req.DateFrom)
        {
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        var start = new DateTimeOffset(req.DateFrom.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(3));
        var end = new DateTimeOffset(req.DateTo.ToDateTime(TimeOnly.MaxValue), TimeSpan.FromHours(3));
        var statusCodes = req.StatusCodes
            .Where(x => x > 0)
            .Distinct()
            .ToArray();

        var query = db.SystemLogs
            .Where(x => x.CreatedAt >= start && x.CreatedAt <= end);

        if (statusCodes.Length > 0)
        {
            query = query.Where(x => statusCodes.Contains(x.StatusCode));
        }

        var logs = await query.ToListAsync(ct);
        if (logs.Count == 0)
        {
            return 0;
        }

        db.SystemLogs.RemoveRange(logs);
        await db.SaveChangesAsync(ct);

        return logs.Count;
    }
}
