using Domain.Base;

namespace Domain.Entities
{
	public class Owner : BaseEntity
	{
		public string ApplicationUserId { get; set; } = null!;
		public ApplicationUser ApplicationUser { get; set; } = null!;
		public Company? Company { get; set; }
		public List<Invoice> Invoices { get; set; } = [];
		public bool WizardCompleted { get; set; }
		public List<OwnerIssueReport> IssueReports { get; set; } = [];
	}
}
