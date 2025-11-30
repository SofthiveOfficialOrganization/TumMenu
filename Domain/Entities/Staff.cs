using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Staff : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Email { get; set; } // Might be used for contact purposes
        public string? PhoneNumber { get; set; } // Might be used for contact purposes

        [MaxLength(100)]
        public string Role { get; set; } = null!; // TODO: Change it to company-specific roles later
        public Guid StoreId { get; set; }
        public Store Store { get; set; } = null!;
    }
}
