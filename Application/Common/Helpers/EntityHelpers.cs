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

		public static void RemoveWhere<T>(this ICollection<T> source, Func<T, bool> predicate)
		where T : class
		{
			var toRemove = source.Where(predicate).ToList();
			foreach(var item in toRemove)
				source.Remove(item);
		}
	}
}
