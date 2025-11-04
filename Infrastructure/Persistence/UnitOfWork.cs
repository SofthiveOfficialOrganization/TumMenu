using Application.Abstractions;
namespace Infrastructure.Persistence
{
	public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
	{
		public Task<int> SaveChangesAsync(CancellationToken ct = default)
			=> db.SaveChangesAsync(ct);
	}
}
