using Domain.Base;

namespace Domain.Entities
{
	public class AdPlacement : BaseEntity
	{
		public Guid AdSlotId { get; set; }
		public AdSlot AdSlot { get; set; } = null!;
		public Guid AdCreativeId { get; set; }
		public AdCreative AdCreative { get; set; } = null!;
		public DateTime? StartAt { get; set; }
		public DateTime? EndAt { get; set; }
		public int? DailyCap { get; set; }
		public bool IsActive { get; set; } = true;
	}
}
