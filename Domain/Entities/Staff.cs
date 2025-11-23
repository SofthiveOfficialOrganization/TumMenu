using Domain.Base;
using System.ComponentModel.DataAnnotations;

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
