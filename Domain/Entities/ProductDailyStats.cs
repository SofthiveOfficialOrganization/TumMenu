using Domain.Base;

namespace Domain.Entities
{
    public class ProductDailyStats : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public DateOnly Day { get; set; }
        public int Views { get; set; }
        public int UniqueIps { get; set; }
    }
}
