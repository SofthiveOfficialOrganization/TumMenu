using Domain.Base;
using System.ComponentModel.DataAnnotations;

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
		public int? EstimatedPreparationTimeInMinutes { get; set; }
		public ICollection<ProductPrice> Prices { get; set; } = [];
		public ICollection<Tag> Tags { get; set; } = [];
		public ICollection<Media> Medias { get; set; } = [];
	}
}
