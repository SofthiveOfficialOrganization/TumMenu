using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public enum StoreSocialPlatform
	{
		Twitter = 0,
		Instagram = 1,
		Facebook = 2,
		YouTube = 3,
		Other = 4
	}

	public class StoreSocialLink : BaseEntity
	{
		public StoreSocialPlatform Platform { get; set; } = StoreSocialPlatform.Other;
		[MaxLength(120)]
		public string? DisplayName { get; set; }
		[MaxLength(500)]
		public string Url { get; set; } = null!;
		public int SortOrder { get; set; }
		public Guid StoreId { get; set; }
		public Store Store { get; set; } = null!;
	}
}
