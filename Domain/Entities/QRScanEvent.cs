using Domain.Base;

namespace Domain.Entities
{
    public class QRScanEvent : BaseEntity
    {
        public Guid QRCodeId { get; set; }
        public QRCode QRCode { get; set; } = null!;
        public DateTime ScannedAt { get; set; }
        public string UserAgent { get; set; } = null!;
        public string IpHash { get; set; } = null!;
        public string? Country { get; set; }
        public string? Referrer { get; set; }
        public string DeviceType { get; set; } = "mobile";
    }
}
