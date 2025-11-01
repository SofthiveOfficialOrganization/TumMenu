using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Base
{
	public interface IBaseEntity
	{
		Guid Id { get; }
		long CreatedOn { get; }
		long? ModifiedOn { get; }
		string CreatedBy { get; }
		string? ModifiedBy { get; }
		bool IsDeleted { get; }
		void Created(string? userId = null);
		void Modified(string? userId, bool isDeleted = false);
		void Deleted(string? userId);
	}
}
