using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Tag : BaseEntity
	{
		[MaxLength(100)] public string Name { get; set; } = null!;

		public Guid ProductId { get; set; }
		public Product Product { get; set; } = null!;
	}
}
