using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class UsageCounter : BaseEntity
	{
		public Guid CompanyId { get; set; }
		public string Key { get; set; } = null!;   // "qr_count_2025_11"
		public int Value { get; set; }  // 25
		public DateOnly Period { get; set; }       // YYYY-MM
	}
}
