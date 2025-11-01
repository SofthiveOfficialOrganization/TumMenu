using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
