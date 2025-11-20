using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Menu : BaseEntity
	{
		public Guid CompanyId { get; set; }
		public Company Company { get; set; } = null!;
		public Guid? MenuTemplateId { get; set; }
		public Guid? StoreId { get; set; }
		public Store? Store { get; set; }
		public MenuStatus Status { get; set; } = MenuStatus.Unknown;
		public DateTime? PublishedAt { get; set; }
		[MaxLength(50)]
		public string? Version { get; set; }
		public ICollection<Image> Images { get; set; } = [];
		public ICollection<Category> Categories { get; set; } = [];
	}
	public enum MenuStatus
	{
		Unknown = 0,
		Active,
		Inactive,
		Draft,
		Archived
	}
}
