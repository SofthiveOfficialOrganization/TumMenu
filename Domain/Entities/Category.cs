using Domain.Base;

namespace Domain.Entities
{
	public class Category : BaseEntity
	{
		public Guid MenuId { get; set; }
		public Menu Menu { get; set; } = null!;
		public Guid CategoryLibraryItemId { get; set; }
		public CategoryLibraryItem CategoryLibraryItem { get; set; } = null!;
		
		// --- Hierarchical Category Support ---
		public Guid? ParentId { get; set; }
		public Category? Parent { get; set; }
		public ICollection<Category> SubCategories { get; set; } = [];
		// -------------------------------------

		// Description moved to CategoryLibraryItem
		public int SortOrder { get; set; }
		public bool IsActive { get; set; } = true;
		public ICollection<Product> Products { get; set; } = [];
		public ICollection<Media> Medias { get; set; } = [];
	}
}
