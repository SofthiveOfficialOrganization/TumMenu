using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class AdClick : BaseEntity
	{
		public Guid AdPlacementId { get; set; }
		public DateTime ClickedAt { get; set; }
		public string IpHash { get; set; } = null!;
	}

}
