using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Owner : BaseEntity
    {
        [MaxLength(50)]
        public string? TaxNo { get; set; }

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public Company? Company { get; set; }

        public List<Invoice> Invoices { get; set; } = [];
    }
}
