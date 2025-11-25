using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Plan : BaseEntity
    {
        [MaxLength(150)] public string Name { get; set; } = null!;
        public int ProductLimit { get; set; }
        public bool AdsEnabled { get; set; }
        public bool ThemeCustomization { get; set; }
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public int WelcomeDiscountCount { get; set; }
        public decimal WelcomeDiscountAmount { get; set; }

        public ICollection<ExtensionPackPlan> ExtensionPacks { get; set; } = [];
        public ICollection<Subscription> Subscriptions { get; set; } = [];
    }
}
