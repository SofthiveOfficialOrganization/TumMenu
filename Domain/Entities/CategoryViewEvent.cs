using Domain.Base;

namespace Domain.Entities
{
    public class CategoryViewEvent : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public DateTime ViewedAt { get; set; }
        public string UserAgent { get; set; } = null!;
        public string IpHash { get; set; } = null!;
        public string DeviceType { get; set; } = "mobile";
    }
}
