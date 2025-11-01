using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Menu : BaseEntity
	{
		public Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;
		public Guid? BannerImageId { get; set; }
		public Image? BannerImage { get; set; }

		public ICollection<Category> Categories { get; set; } = [];
	}
}
