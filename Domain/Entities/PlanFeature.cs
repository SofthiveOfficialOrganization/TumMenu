using Domain.Base;

namespace Domain.Entities
{
    public class PlanFeature : BaseEntity
    {
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = null!;
        public string Key { get; set; } = null!;   // "max_qr","max_store","ad_free"...
        public string Value { get; set; } = null!;
    }
}
