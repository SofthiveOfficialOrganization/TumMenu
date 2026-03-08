using Domain.Base;

namespace Domain.Entities
{
    public class CategoryDailyStats : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public DateOnly Day { get; set; }
        public int Views { get; set; }
        public int UniqueIps { get; set; }
    }
}
