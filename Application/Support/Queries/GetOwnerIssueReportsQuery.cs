using Application.Abstractions;
using Application.Support.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Support.Queries;

/// <summary>
/// Admin için tüm issue raporlarını listeler.
/// </summary>
public sealed record GetOwnerIssueReportsQuery(
    IssueReportStatus? Status = null
) : IRequest<OwnerIssueReportListDTO>;

public sealed class GetOwnerIssueReportsHandler(
    IRepository<OwnerIssueReport> repo
) : IRequestHandler<GetOwnerIssueReportsQuery, OwnerIssueReportListDTO>
{
    public async Task<OwnerIssueReportListDTO> Handle(GetOwnerIssueReportsQuery req, CancellationToken ct)
    {
        var summary = await repo.Query()
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalCount = g.Count(),
                OpenCount = g.Count(r => r.Status == IssueReportStatus.Open),
                InProgressCount = g.Count(r => r.Status == IssueReportStatus.InProgress),
                ResolvedCount = g.Count(r => r.Status == IssueReportStatus.Resolved),
                ClosedCount = g.Count(r => r.Status == IssueReportStatus.Closed)
            })
            .FirstOrDefaultAsync(ct);

        var query = repo.Query()
            .Include(r => r.Owner)
                .ThenInclude(o => o.ApplicationUser)
            .Include(r => r.Owner)
                .ThenInclude(o => o.Company)
            .AsNoTracking();
        if (req.Status.HasValue)
            query = query.Where(r => r.Status == req.Status.Value);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new OwnerIssueReportDTO
            {
                Id = r.Id,
                OwnerId = r.OwnerId,
                OwnerName = r.Owner.ApplicationUser.UserName ?? "-",
                OwnerEmail = r.Owner.ApplicationUser.Email ?? "-",
                CompanyTitle = r.Owner.Company != null ? r.Owner.Company.Title : null,
                Description = r.Description,
                AttemptedAction = r.AttemptedAction,
                CurrentPageTitle = r.CurrentPageTitle,
                CurrentPageUrl = r.CurrentPageUrl,
                BrowserInfo = r.BrowserInfo,
                Viewport = r.Viewport,
                Status = r.Status,
                AdminNote = r.AdminNote,
                CreatedAt = r.CreatedAt,
                ResolvedAt = r.ResolvedAt
            })
            .ToListAsync(ct);

        return new OwnerIssueReportListDTO
        {
            Reports = reports,
            TotalCount = summary?.TotalCount ?? 0,
            OpenCount = summary?.OpenCount ?? 0,
            InProgressCount = summary?.InProgressCount ?? 0,
            ResolvedCount = summary?.ResolvedCount ?? 0,
            ClosedCount = summary?.ClosedCount ?? 0
        };
    }
}
