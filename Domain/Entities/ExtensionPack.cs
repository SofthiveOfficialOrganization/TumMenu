using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class ExtensionPack : BaseEntity
	{
		[MaxLength(150)] public string Name { get; set; } = null!;
		public decimal Price { get; set; }
		public List<ExtensionPackPlan> ExtensionPackPlans { get; set; } = [];
	}
}
