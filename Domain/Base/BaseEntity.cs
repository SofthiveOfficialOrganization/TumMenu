using Domain.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Base
{
	public interface IBaseEntity : IAuditable
	{
		Guid Id { get; }
		void Created(string? userId = null);
		void Modified(string? userId, bool isDeleted = false);
		void Deleted(string? userId);
	}
	public class BaseEntity : IBaseEntity
	{
		[Key]
		public Guid Id { get; set; }

		public long CreatedOn { get; set; }
		public long? ModifiedOn { get; set; }
		public string? CreatedBy { get; set; }
		public string? ModifiedBy { get; set; }
		public bool IsDeleted { get; set; }

		public void Created(string? userId = null)
		{
			CreatedOn = DateTimeHelper.GetUtcNowTime();
			CreatedBy = userId ?? string.Empty;
			Id = Guid.NewGuid();
		}

		public void Modified(string? userId = null, bool isDeleted = false)
		{
			ModifiedOn = DateTimeHelper.GetUtcNowTime();
			ModifiedBy = userId;
			if(isDeleted) Deleted(userId);
		}

		public void Deleted(string? userId = null)
		{
			IsDeleted = true;
			ModifiedOn = DateTimeHelper.GetUtcNowTime();
			ModifiedBy = userId;
		}
	}
}
