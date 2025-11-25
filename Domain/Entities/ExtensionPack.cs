using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ExtensionPack : BaseEntity
    {
        [MaxLength(150)] public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public List<ExtensionPackPlan> ExtensionPackPlans { get; set; } = [];
    }
}
