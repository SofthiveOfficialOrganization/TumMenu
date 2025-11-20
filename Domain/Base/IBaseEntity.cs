using System;
using System.Collections.Generic;
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
}
