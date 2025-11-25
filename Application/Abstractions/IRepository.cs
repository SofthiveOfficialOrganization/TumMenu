using Application.Common.Base.Page;
using Application.Common.Base.Page.RequestBase;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Abstractions;

public interface IRepository<T> where T : class
{
	IQueryable<T> Query(bool tracked = false);
	Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task AddAsync(T entity, CancellationToken ct = default);
	void Update(T entity);
	void SoftDelete(T entity);
	Task<IPaginate<T>> GetPageListAsync(PageRequest request, Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, bool enableTracking = true, CancellationToken ct = default);
}