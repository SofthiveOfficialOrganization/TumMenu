using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Common.Interfaces;
using Application.SystemLogs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.SystemLogs.Queries;

public sealed class GetSystemLogsQuery : PageRequest, IRequest<PaginatedListDTO<SystemLogListDTO>>
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public TimeOnly? TimeFrom { get; set; }
    public TimeOnly? TimeTo { get; set; }
    public int? StatusCode { get; set; }
    public string? User { get; set; }
    public string? Search { get; set; }

    public GetSystemLogsQuery()
    {
        PageSize = 25;
    }
}

public sealed class GetSystemLogsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSystemLogsQuery, PaginatedListDTO<SystemLogListDTO>>
{
    public async Task<PaginatedListDTO<SystemLogListDTO>> Handle(GetSystemLogsQuery req, CancellationToken ct)
    {
        var query = db.SystemLogs.AsNoTracking();

        var hasExplicitDateTimeFilter =
            req.DateFrom.HasValue ||
            req.DateTo.HasValue ||
            req.TimeFrom.HasValue ||
            req.TimeTo.HasValue;

        if (!hasExplicitDateTimeFilter)
        {
            var start = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddDays(-1);
            query = query.Where(x => x.CreatedAt >= start);
        }

        if (req.DateFrom.HasValue)
        {
            var start = req.DateFrom.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.CreatedAt >= new DateTimeOffset(start, TimeSpan.FromHours(3)));
        }

        if (req.DateTo.HasValue)
        {
            var end = req.DateTo.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(x => x.CreatedAt <= new DateTimeOffset(end, TimeSpan.FromHours(3)));
        }

        if (req.TimeFrom.HasValue)
        {
            var from = req.TimeFrom.Value.ToTimeSpan();
            query = query.Where(x => x.CreatedAt.TimeOfDay >= from);
        }

        if (req.TimeTo.HasValue)
        {
            var to = req.TimeTo.Value.ToTimeSpan();
            query = query.Where(x => x.CreatedAt.TimeOfDay <= to);
        }

        if (req.StatusCode.HasValue)
        {
            query = query.Where(x => x.StatusCode == req.StatusCode.Value);
        }

        if (!string.IsNullOrWhiteSpace(req.User))
        {
            var user = req.User.Trim();
            query = query.Where(x =>
                (x.UserId != null && x.UserId.Contains(user)) ||
                (x.UserName != null && x.UserName.Contains(user)));
        }

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.Trim();
            query = query.Where(x =>
                (x.ResponseMessage != null && x.ResponseMessage.Contains(search)) ||
                (x.ExceptionMessage != null && x.ExceptionMessage.Contains(search)) ||
                (x.ExceptionType != null && x.ExceptionType.Contains(search)) ||
                (x.Path != null && x.Path.Contains(search)) ||
                (x.TraceId != null && x.TraceId.Contains(search)) ||
                (x.StackTrace != null && x.StackTrace.Contains(search)));
        }

        var count = await query.CountAsync(ct);
        var page = Math.Max(req.Page, req.From);
        var pageSize = Math.Clamp(req.PageSize, 1, 100);
        var pages = (int)Math.Ceiling(count / (double)pageSize);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - req.From) * pageSize)
            .Take(pageSize)
            .Select(x => new SystemLogListDTO(
                x.Id,
                x.CreatedAt,
                x.Level,
                x.Source,
                x.StatusCode,
                x.UserName,
                x.UserId,
                x.Path,
                x.ResponseMessage,
                x.ExceptionMessage,
                x.TraceId))
            .ToListAsync(ct);

        return new PaginatedListDTO<SystemLogListDTO>
        {
            Items = items,
            Index = page,
            Size = pageSize,
            From = req.From,
            Count = count,
            Pages = pages,
            HasPrevious = page > req.From,
            HasNext = page < pages + req.From - 1
        };
    }
}
