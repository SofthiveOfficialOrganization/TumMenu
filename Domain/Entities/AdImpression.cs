using Domain.Base;

namespace Domain.Entities
{
	public class AdImpression : BaseEntity
	{
		public Guid AdPlacementId { get; set; }
		public DateTime ShownAt { get; set; }
		public string IpHash { get; set; } = null!;
	}
}
