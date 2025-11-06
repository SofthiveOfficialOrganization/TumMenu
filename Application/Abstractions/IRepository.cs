namespace Application.Abstractions;

public interface IRepository<T> where T : class
{
	IQueryable<T> Query(bool tracked = false);
	Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
	Task AddAsync(T entity, CancellationToken ct);
	void Update(T entity);
	void Remove(T entity);
}