using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Support.Commands;

public sealed record UpdateIssueStatusCommand(
    Guid ReportId,
    IssueReportStatus Status,
    string? AdminNote
) : IRequest<UpdateIssueStatusResult>, ITransactionalRequest;

public sealed record UpdateIssueStatusResult(
    bool JustResolved,
    string OwnerEmail,
    string OwnerName,
    string? CompanyTitle,
    string Description,
    string? AdminNote
);

public sealed class UpdateIssueStatusHandler(
    IRepository<OwnerIssueReport> repo
) : IRequestHandler<UpdateIssueStatusCommand, UpdateIssueStatusResult>
{
    public async Task<UpdateIssueStatusResult> Handle(UpdateIssueStatusCommand req, CancellationToken ct)
    {
        var report = await repo.Query()
            .Include(r => r.Owner)
                .ThenInclude(o => o.ApplicationUser)
            .Include(r => r.Owner)
                .ThenInclude(o => o.Company)
            .FirstOrDefaultAsync(r => r.Id == req.ReportId, ct)
            ?? throw new NotFoundAppException("Bildirim bulunamadı.");

        var wasResolved = report.ResolvedAt != null;

        report.Status = req.Status;
        report.AdminNote = req.AdminNote;

        if (req.Status == IssueReportStatus.Resolved && report.ResolvedAt == null)
            report.ResolvedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));

        report.Modified();
        repo.Update(report);
        await repo.SaveChangesAsync(ct);

        var justResolved = req.Status == IssueReportStatus.Resolved && !wasResolved;

        return new UpdateIssueStatusResult(
            JustResolved: justResolved,
            OwnerEmail: report.Owner.ApplicationUser?.Email ?? string.Empty,
            OwnerName: report.Owner.ApplicationUser?.UserName ?? "-",
            CompanyTitle: report.Owner.Company?.Title,
            Description: report.Description,
            AdminNote: req.AdminNote
        );
    }
}
