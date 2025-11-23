using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class PaymentMethod : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public bool IsDefault { get; set; }

        [MaxLength(50)] public string? Psp { get; set; }
        [MaxLength(100)] public string? PspCustomerId { get; set; }
        [MaxLength(50)] public string? CardBrand { get; set; }
        [MaxLength(4)] public string? CardLast4 { get; set; }
        public int? ExpMonth { get; set; }
        public int? ExpYear { get; set; }
        [MaxLength(200)] public string? BillingName { get; set; }
        [MaxLength(320)] public string? BillingEmail { get; set; }
        [MaxLength(50)] public string? BillingPhone { get; set; }
    }

}
