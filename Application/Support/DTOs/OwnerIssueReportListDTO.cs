namespace Application.Support.DTOs;

public class OwnerIssueReportListDTO
{
    public List<OwnerIssueReportDTO> Reports { get; set; } = [];
    public int TotalCount { get; set; }
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
}
