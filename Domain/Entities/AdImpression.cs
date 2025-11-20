using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class AdImpression : BaseEntity
	{
		public Guid AdPlacementId { get; set; }
		public DateTime ShownAt { get; set; }
		public string IpHash { get; set; } = null!;
	}
}
