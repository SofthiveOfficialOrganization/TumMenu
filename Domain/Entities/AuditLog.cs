using Domain.Base;

namespace Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public Guid EntityId { get; set; }
        public string ChangesJson { get; set; } = "{}";
        public string? Ip { get; set; }
    }

}
