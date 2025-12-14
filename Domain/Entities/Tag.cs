using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Tag : BaseEntity
	{
		[MaxLength(100)] public string Name { get; set; } = null!;

		public ICollection<ProductTag> ProductTags { get; set; } = [];
	}
}
