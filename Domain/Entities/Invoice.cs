using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Invoice : BaseEntity
	{
		public Guid CompanyId { get; set; }
		public Guid SubscriptionId { get; set; }
		public Subscription Subscription { get; set; } = null!;
		public string Number { get; set; } = null!;

		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
		public DateTime? DueDate { get; set; }
		public DateTime? PaidAt { get; set; }
		public DateTime? RefundedAt { get; set; }
		public string Currency { get; set; } = "TRY";
		public PaymentStatus PaymentStatus { get; set; }

		[MaxLength(100)] public string? PspPaymentId { get; set; }
		public ICollection<InvoiceLine> Lines { get; set; } = [];
	}

	public enum PaymentStatus
	{
		Pending = 0,
		Paid = 1,
		Failed = 2,
		Refunded = 3
	}
}
