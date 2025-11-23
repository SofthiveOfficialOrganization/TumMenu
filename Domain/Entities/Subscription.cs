using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Subscription : BaseEntity
	{
		public Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;

		public Guid PlanId { get; set; }
		public Plan Plan { get; set; } = null!;

		public Guid? OwnerId { get; set; }
		public Owner? Owner { get; set; }

		public DateTime StartAt { get; set; }
		public DateTime? CurrentPeriodEnd { get; set; } // end date of the current billing period
		public bool CancelAtPeriodEnd { get; set; } = false; // if true, subscription will be cancelled at the end of the current period
		public DateTime? CancelledAt { get; set; }
		public BillingCycle BillingCycle { get; set; }
		public SubscriptionStatus Status { get; set; }

		[MaxLength(100)] public string? PspSubscriptionId { get; set; }

		public ICollection<Invoice> Invoices { get; set; } = [];
	}

	public enum BillingCycle
	{
		Monthly = 1,
		Yearly = 2
	}

	public enum SubscriptionStatus
	{
		Active = 1,
		Cancelled = 2,
		Paused = 3
	}
}
