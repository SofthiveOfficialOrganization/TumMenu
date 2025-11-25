using Domain.Base;

namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid? CompanyId { get; set; }
        public string Channel { get; set; } = "email"; // email/push/inapp
        public string TemplateKey { get; set; } = null!;
        public string PayloadJson { get; set; } = "{}";
        public DateTimeOffset? ScheduledAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public string? ToUserId { get; set; }
    }
}
