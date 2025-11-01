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
		public Guid SubscriptionId { get; set; }
		public Subscription Subscription { get; set; } = null!;

		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
		public decimal SubtotalAmount { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public PaymentStatus PaymentStatus { get; set; }

		[MaxLength(100)] public string? PspPaymentId { get; set; }

		public Guid? OwnerId { get; set; }
		public Owner? Owner { get; set; }
	}

	public enum PaymentStatus
	{
		Pending = 0,
		Paid = 1,
		Failed = 2,
		Refunded = 3
	}
}
