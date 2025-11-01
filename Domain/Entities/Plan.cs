using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
