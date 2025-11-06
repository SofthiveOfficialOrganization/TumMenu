using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
	public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
	public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
	{
		var strategy = db.Database.CreateExecutionStrategy();
		await strategy.ExecuteAsync(async () =>
		{
			await using var tx = await db.Database.BeginTransactionAsync(ct);
			await action(ct);
			await db.SaveChangesAsync(ct);
			await tx.CommitAsync(ct);
		});
	}
}