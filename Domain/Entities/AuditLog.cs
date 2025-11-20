using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class AuditLog : BaseEntity
	{
		public string UserId { get; set; } = null!;
		public string Action { get; set; } = null!;
		public string Entity { get; set; } = null!;
		public Guid EntityId { get; set; }
		public string ChangesJson { get; set; } = "{}";
		public DateTimeOffset CreatedAt { get; set; }
		public string? Ip { get; set; }
	}

}
