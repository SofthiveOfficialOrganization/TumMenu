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
		public Guid? CompanyId { get; set; }
		public Company? Company { get; set; }
		public Guid? StoreId { get; set; }
		public Store? Store { get; set; }
		public Guid? MenuId { get; set; }
		public Menu? Menu { get; set; }

		[MaxLength(50)] public string PublicKey { get; set; } = null!; // /q/{key}
		[MaxLength(2048)] public string TargetUrl { get; set; } = null!;
		public bool IsDynamic { get; set; } = true;
		public bool IsActive { get; set; } = true;
		public string ECCLevel { get; set; } = "M";
		public string? StyleJson { get; set; }
	}
}
