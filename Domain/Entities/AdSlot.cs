using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class AdSlot : BaseEntity
	{
		public string Key { get; set; } = null!;   // "store_header","menu_inline"
		public string Description { get; set; } = "";
		public bool IsActive { get; set; } = true;
	}

}
