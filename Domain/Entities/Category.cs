using Domain.Base;

namespace Domain.Entities
{
	public class Category : BaseEntity
	{
		public Guid MenuId { get; set; }
		public Menu Menu { get; set; } = null!;
		public Guid CategoryLibraryItemId { get; set; }
		public CategoryLibraryItem CategoryLibraryItem { get; set; } = null!;
		public string Description { get; set; } = string.Empty;
		public int SortOrder { get; set; }
		public bool IsActive { get; set; } = true;
		public ICollection<Product> Products { get; set; } = [];
		public ICollection<Media> Medias { get; set; } = [];
	}
}
