using Application.Common.Interfaces;
using Application.SystemLogs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.SystemLogs.Queries;

public sealed record GetSystemLogDetailQuery(Guid Id) : IRequest<SystemLogDetailDTO?>;

public sealed class GetSystemLogDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSystemLogDetailQuery, SystemLogDetailDTO?>
{
    public Task<SystemLogDetailDTO?> Handle(GetSystemLogDetailQuery req, CancellationToken ct)
    {
        return db.SystemLogs
            .AsNoTracking()
            .Where(x => x.Id == req.Id)
            .Select(x => new SystemLogDetailDTO(
                x.Id,
                x.CreatedAt,
                x.Level,
                x.Source,
                x.StatusCode,
                x.ErrorCode,
                x.ResponseMessage,
                x.ExceptionType,
                x.ExceptionMessage,
                x.StackTrace,
                x.TraceId,
                x.HttpMethod,
                x.Path,
                x.QueryString,
                x.UserId,
                x.UserName,
                x.RemoteIp,
                x.UserAgent))
            .FirstOrDefaultAsync(ct);
    }
}
