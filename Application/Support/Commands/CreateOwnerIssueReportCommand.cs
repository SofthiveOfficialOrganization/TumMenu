using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;

namespace Application.Support.Commands;

public sealed record CreateOwnerIssueReportCommand(
    string Description,
    string? AttemptedAction,
    string CurrentPageTitle,
    string CurrentPageUrl,
    string? BrowserInfo,
    string? Viewport
) : IRequest<Guid>, ITransactionalRequest;

public sealed class CreateOwnerIssueReportHandler(
    IRepository<OwnerIssueReport> repo,
    IUserContext userContext
) : IRequestHandler<CreateOwnerIssueReportCommand, Guid>
{
    public async Task<Guid> Handle(CreateOwnerIssueReportCommand req, CancellationToken ct)
    {
        var ownerId = userContext.OwnerIdParsed
            ?? throw new ForbiddenAppException("Yönetici yetkisi bulunamadı.");

        var report = new OwnerIssueReport
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Description = req.Description,
            AttemptedAction = req.AttemptedAction,
            CurrentPageTitle = req.CurrentPageTitle,
            CurrentPageUrl = req.CurrentPageUrl,
            BrowserInfo = req.BrowserInfo,
            Viewport = req.Viewport,
            Status = IssueReportStatus.Open
        };
        report.Created(userContext.UserId);

        await repo.AddAsync(report, ct);
        await repo.SaveChangesAsync(ct);

        return report.Id;
    }
}
