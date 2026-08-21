using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class QrOrderSession : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid QRCodeId { get; set; }
    public QRCode QRCode { get; set; } = null!;

    [MaxLength(128)]
    public string TokenHash { get; set; } = null!;

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }

    [MaxLength(128)]
    public string? CreatedIp { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public bool IsRevoked { get; set; }
}
