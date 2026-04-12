using Domain.Base;

namespace Domain.Entities;

public class OwnerIssueReport : BaseEntity
{
    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public string Description { get; set; } = null!;
    public string? AttemptedAction { get; set; }
    public string CurrentPageTitle { get; set; } = null!;
    public string CurrentPageUrl { get; set; } = null!;
    public string? BrowserInfo { get; set; }
    public string? Viewport { get; set; }

    public IssueReportStatus Status { get; set; } = IssueReportStatus.Open;
    public string? AdminNote { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}

public enum IssueReportStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}
