using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Store : BaseEntity
	{
		[MaxLength(200)]
		public string Name { get; set; } = null!;

		[MaxLength(50)]
		public string Slug { get; set; } = null!;

		public Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;
		public Address? Address { get; set; }
		public QRCode? QRCode { get; set; }
		public Menu? Menu { get; set; }

		public ICollection<Staff> Staffs { get; set; } = [];
		public ICollection<Image> Images { get; set; } = [];
	}
}