using Domain.Base;

namespace Domain.Entities
{
    public class ProductViewEvent : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public DateTime ViewedAt { get; set; }
        public string UserAgent { get; set; } = null!;
        public string IpHash { get; set; } = null!;
        public string DeviceType { get; set; } = "mobile";
    }
}
