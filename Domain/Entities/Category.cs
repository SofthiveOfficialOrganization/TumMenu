using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Category : BaseEntity
	{
		public Guid MenuId { get; set; }
		public Menu Menu { get; set; } = null!;
		public int SortOrder { get; set; }

		public ICollection<Product> Products { get; set; } = [];
		public ICollection<Image> Images { get; set; } = [];
	}
}
