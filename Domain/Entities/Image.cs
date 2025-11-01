using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Image : BaseEntity
	{
		[MaxLength(1024)] public string ImageLink { get; set; } = null!;
		[MaxLength(300)] public string? AltText { get; set; }
		public int SortOrder { get; set; }

		// Polimorfik ilişki
		public Guid ReferenceId { get; set; }
		public ImageRefType Type { get; set; }

		public int? Width { get; set; }
		public int? Height { get; set; }
	}
	public enum ImageRefType
	{
		Company = 1,
		Menu = 2,
		Category = 3,
		Product = 4,
		QRCode = 5
	}
}
