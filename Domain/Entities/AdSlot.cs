using Domain.Base;

namespace Domain.Entities
{
    public class AdSlot : BaseEntity
    {
        public string Key { get; set; } = null!;   // "store_header","menu_inline"
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

}
