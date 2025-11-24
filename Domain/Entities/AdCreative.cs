using Domain.Base;

namespace Domain.Entities
{
    public class AdCreative : BaseEntity
    {
        public string Type { get; set; } = "image"; // "image","html","third_party"
        public string Content { get; set; } = null!; // URL or HTML snippet
        public string? ClickUrl { get; set; }
    }
}
