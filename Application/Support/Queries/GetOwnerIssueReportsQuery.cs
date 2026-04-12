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
) : IRequest<List<OwnerIssueReportDTO>>;

public sealed class GetOwnerIssueReportsHandler(
    IRepository<OwnerIssueReport> repo
) : IRequestHandler<GetOwnerIssueReportsQuery, List<OwnerIssueReportDTO>>
{
    public async Task<List<OwnerIssueReportDTO>> Handle(GetOwnerIssueReportsQuery req, CancellationToken ct)
    {
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

        return reports;
    }
}
