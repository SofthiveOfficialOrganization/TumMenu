using Domain.Base;

namespace Domain.Entities
{
    public class WebhookEvent : BaseEntity
    {
        public string Provider { get; set; } = null!;
        public string ProviderEventId { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime ReceivedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
