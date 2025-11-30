using Domain.Base;

namespace Domain.Entities
{
    public class Owner : BaseEntity
    {
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public Company? Company { get; set; }

        public List<Invoice> Invoices { get; set; } = [];
    }
}
