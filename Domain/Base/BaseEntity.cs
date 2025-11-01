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
		public long CreatedOn { get; private set; }
		public DateTime CreatedOnValue { get { return DateTimeHelper.ConvertLocalDateTime(CreatedOn); } }
		public long? ModifiedOn { get; private set; }
		public DateTime? ModifiedOnValue { get { return ModifiedOn.HasValue ? DateTimeHelper.ConvertLocalDateTime(ModifiedOn.Value) : null; } }

		[MaxLength(50)]
		public string CreatedBy { get; private set; }

		[MaxLength(50)]
		public string? ModifiedBy { get; private set; }
		public bool IsDeleted { get; private set; }

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

			if(isDeleted)
			{
				Deleted(userId);
			}
		}

		public void Deleted(string? userId = null)
		{
			IsDeleted = true;
			ModifiedOn = DateTimeHelper.GetUtcNowTime();
			ModifiedBy = userId;
		}
	}
}
