using Domain.Entities;

namespace Application.Support.DTOs;

public class OwnerIssueReportDTO
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public string OwnerEmail { get; set; } = null!;
    public string? CompanyTitle { get; set; }

    public string Description { get; set; } = null!;
    public string? AttemptedAction { get; set; }
    public string CurrentPageTitle { get; set; } = null!;
    public string CurrentPageUrl { get; set; } = null!;
    public string? BrowserInfo { get; set; }
    public string? Viewport { get; set; }

    public IssueReportStatus Status { get; set; }
    public string? AdminNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
