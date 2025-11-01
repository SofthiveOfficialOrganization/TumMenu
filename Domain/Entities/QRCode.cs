using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class QRCode : BaseEntity
	{
		[MaxLength(1024)] public string? ImageLink { get; set; }
		[MaxLength(300)] public string? Value { get; set; }

		public Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;

		public string? StyleJson { get; set; }
	}
}
