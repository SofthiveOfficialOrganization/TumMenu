using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Subscription : BaseEntity
	{

		public Guid PlanId { get; set; }
		public Plan Plan { get; set; } = null!;

		public Guid? OwnerId { get; set; }
		public Owner? Owner { get; set; }

		public DateTime StartAt { get; set; }
		public DateTime? NextBillingAt { get; set; }
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
