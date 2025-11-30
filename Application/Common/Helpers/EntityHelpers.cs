using Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Helpers
{
	public static class EntityHelpers
	{

		public static T EnsureFound<T>(this T? entity, string message)
		where T : class
		{
			if(entity is null)
				throw new NotFoundAppException(message);

			return entity;
		}
	}
}
