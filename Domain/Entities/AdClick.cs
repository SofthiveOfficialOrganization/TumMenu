using Domain.Base;

namespace Domain.Entities
{
    public class AdClick : BaseEntity
    {
        public Guid AdPlacementId { get; set; }
        public DateTime ClickedAt { get; set; }
        public string IpHash { get; set; } = null!;
    }

}
