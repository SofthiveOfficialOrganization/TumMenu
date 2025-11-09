using Application.Abstractions;
using Domain.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence;

public sealed class AuditInterceptor(IUserContext user) : SaveChangesInterceptor
{
	public override InterceptionResult<int> SavingChanges(
		DbContextEventData e, InterceptionResult<int> result)
	{
		Stamp(e.Context);
		return result;
	}

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
	{
		Stamp(e.Context);
		return new(result);
	}

	private void Stamp(DbContext? ctx)
	{
		if(ctx is null) return;
		var userId = user.UserId ?? "system";

		foreach(var entry in ctx.ChangeTracker.Entries<IBaseEntity>())
		{
			switch(entry.State)
			{
				case EntityState.Added:
					entry.Entity.Created(userId);
					break;

				case EntityState.Modified:
					entry.Entity.Modified(userId);
					break;

				case EntityState.Deleted: // SOFT DELETE
					entry.State = EntityState.Modified;
					entry.Entity.Deleted(userId);
					break;
			}
		}
	}
}

