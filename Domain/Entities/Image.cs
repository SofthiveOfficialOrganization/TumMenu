using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Image : BaseEntity
	{
		[MaxLength(1024)] public string ImageLink { get; set; } = null!;
		[MaxLength(300)] public string? AltText { get; set; }
		public int SortOrder { get; set; }
		public Guid ReferenceId { get; set; }
		public ImageRefType Type { get; set; } = ImageRefType.Unknown;
		[MaxLength(50)]
		public string Slot { get; set; } = "default-gallery";
		public int? Width { get; set; }
		public int? Height { get; set; }
	}
	public enum ImageRefType
	{
		Unknown = 0,
		Company,
		Store,
		Menu,
		Category,
		Product,
		QRCode
	}
}
