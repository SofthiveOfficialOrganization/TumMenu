using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Media : BaseEntity
	{
		[MaxLength(1024)] public string MediaUrl { get; set; } = null!;
		[MaxLength(300)] public string? AltText { get; set; }
		public int SortOrder { get; set; }
		public Guid? CompanyId { get; set; }
		public Guid ReferenceId { get; set; }
		public MediaRefType Type { get; set; } = MediaRefType.Unknown;
		public MediaKind Kind { get; set; } = MediaKind.Unknown;

		[MaxLength(50)]
		public string Slot { get; set; } = "default-gallery";
		public int? Width { get; set; }
		public int? Height { get; set; }
		public long? FileSize { get; set; }
		[MaxLength(20)]
		public string? Extension { get; set; }
		[MaxLength(100)]
		public string? MimeType { get; set; }

		// For CategoryLibraryItem, we use a separate field to avoid foreign key conflicts
		public Guid? CategoryLibraryItemId { get; set; }
		public CategoryLibraryItem? CategoryLibraryItem { get; set; }

		// Distinct foreign keys for parent relationships
		public Guid? StoreId { get; set; }
		public Guid? MenuId { get; set; }
		public Guid? ProductId { get; set; }
	}
	public enum MediaRefType
	{
		Unknown = 0,
		Company,
		Store,
		Menu,
		Category,
		Product,
		QRCode,
		CategoryLibraryItem
	}
	public enum MediaKind
	{
		Unknown = 0,
		Image,
		Video,
		Audio,
		Document,
		Other
	}
}
