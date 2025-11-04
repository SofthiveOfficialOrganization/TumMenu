using Domain.Base;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Common;

public abstract class BaseRepository<TEntity> where TEntity : BaseEntity
{
	protected readonly ApplicationDbContext _db;
	protected readonly DbSet<TEntity> _set;

	protected BaseRepository(ApplicationDbContext db)
	{
		_db = db;
		_set = db.Set<TEntity>();
	}

	public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct)
		=> _set.FirstOrDefaultAsync(e => e.Id == id, ct);

	public virtual Task AddAsync(TEntity entity, CancellationToken ct)
		=> _set.AddAsync(entity, ct).AsTask();

	public virtual void Update(TEntity entity) => _set.Update(entity);
	public virtual void Remove(TEntity entity) => _set.Remove(entity);

	public virtual Task<bool> ExistsAsync(Guid id, CancellationToken ct)
		=> _set.AnyAsync(e => e.Id == id, ct);
}
