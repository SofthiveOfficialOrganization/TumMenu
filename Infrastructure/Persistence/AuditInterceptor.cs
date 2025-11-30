using Application.Abstractions;
using Domain.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence;

public sealed class AuditInterceptor(IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData e, InterceptionResult<int> result)
    {
        System.Diagnostics.Debug.WriteLine(">>> SavingChanges interceptor hit");
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
        var userId = userContext.UserId ?? "system";

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

