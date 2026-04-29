using Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class CategoryLibraryItem : BaseEntity
	{
		public string Title { get; set; } = null!;
		public string Slug { get; set; } = null!;
		public string? Description { get; set; }
		public string? IconKey { get; set; }
		public ICollection<Media> Medias { get; set; } = [];
	}
}
