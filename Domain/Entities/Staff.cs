using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Domain.Entities
{
	public class Staff : BaseEntity
	{
		[MaxLength(100)]
		public string Role { get; set; } = null!;

		public Guid StoreId { get; set; }
		public Store Store { get; set; } = null!;

		public string ApplicationUserId { get; set; } = null!;
		public ApplicationUser User { get; set; } = null!;
	}
}
