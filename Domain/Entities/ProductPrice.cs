using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ProductPrice : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        [MaxLength(100)] public string? Size { get; set; }
        public decimal Price { get; set; }
    }
}
