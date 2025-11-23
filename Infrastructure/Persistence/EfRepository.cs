using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class EfRepository<T>(ApplicationDbContext ctx) : IRepository<T> where T : class
{
	public IQueryable<T> Query(bool tracked = false) =>
		tracked ? ctx.Set<T>() : ctx.Set<T>().AsNoTracking();

	public Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
		ctx.Set<T>().FindAsync([id], ct).AsTask();

	public Task AddAsync(T entity, CancellationToken ct) =>
		ctx.Set<T>().AddAsync(entity, ct).AsTask();

	public void Update(T entity) => ctx.Set<T>().Update(entity);
	public void SoftDelete(T entity) => ctx.Set<T>().Remove(entity);
}
