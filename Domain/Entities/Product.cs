using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Product : BaseEntity, ISluggable
	{
		public Guid CategoryId { get; set; }
		public Category Category { get; set; } = null!;

		[MaxLength(200)] public string Name { get; set; } = null!;
		[MaxLength(200)] public string Slug { get; set; } = null!;
		public string? Description { get; set; }
		public decimal BasePrice { get; set; }
		public int SortOrder { get; set; }
		public bool IsActive { get; set; }
		[MaxLength(300)] public string? Allergens { get; set; }
		public bool? IsVegan { get; set; }
		public bool? IsVegetarian { get; set; }

		public ICollection<ProductPrice> Prices { get; set; } = [];
		public ICollection<Tag> Tags { get; set; } = [];
		public ICollection<Image> Images { get; set; } = [];
	}
}
