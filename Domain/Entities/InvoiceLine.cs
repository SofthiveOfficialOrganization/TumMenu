using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class InvoiceLine : BaseEntity
	{
		public Guid InvoiceId { get; set; }
		public Invoice Invoice { get; set; } = null!;
		public string Description { get; set; } = null!;
		public int Quantity { get; set; } = 1;
		public decimal UnitPrice { get; set; }
		public decimal TaxRate { get; set; }
		public decimal LineTotal { get; set; }
	}
}
