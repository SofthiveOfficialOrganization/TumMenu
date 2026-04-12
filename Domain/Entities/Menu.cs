using Domain.Base;

namespace Domain.Entities
{
	public class Menu : BaseEntity
	{
		public string Title { get; set; } = null!;
		public Guid? CompanyId { get; set; }
		public Company? Company { get; set; }
		public Guid? MenuTemplateId { get; set; }
		public Guid? StoreId { get; set; }
		public Store? Store { get; set; }
		public MenuStatus Status { get; set; } = MenuStatus.Unknown;
		public DateTime? PublishedAt { get; set; }
		public long Version { get; set; } = 1;
		public ICollection<Media> Medias { get; set; } = [];
		public ICollection<Category> Categories { get; set; } = [];
	}
	public enum MenuStatus
	{
		Unknown = 0,
		Active,
		Inactive,
		Draft,
		Archived,
		MainMenu = 5
	}
}
