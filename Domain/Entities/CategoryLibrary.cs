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
		public Media? Media { get; set; }
		public Guid? ParentId { get; set; }
		public CategoryLibraryItem? Parent { get; set; }
		public ICollection<CategoryLibraryItem> Children { get; set; } = [];
	}
}
