using Domain.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Base
{
	public class BaseEntity : IBaseEntity
	{
		[Key]
		public Guid Id { get; set; }

		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? ModifiedAt { get; set; }
		public string? CreatedBy { get; set; }
		public string? ModifiedBy { get; set; }
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public void Created(string? userId = null)
		{
			CreatedAt = DateTimeOffset.UtcNow;
			CreatedBy = userId ?? string.Empty;
			Id = Guid.NewGuid();
		}

		public void Modified(string? userId = null, bool isDeleted = false)
		{
			ModifiedAt = DateTimeOffset.UtcNow;
			ModifiedBy = userId;
			if(isDeleted) Deleted(userId);
		}

		public void Deleted(string? userId = null)
		{
			IsDeleted = true;
			DeletedAt = DateTimeOffset.UtcNow;
			ModifiedBy = userId;
			ModifiedAt = DeletedAt;
		}
	}
}
