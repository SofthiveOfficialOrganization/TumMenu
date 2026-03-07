using Application.Abstractions;
using Application.Common.Base.Page;
using Application.Common.Base.Page.RequestBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq.Expressions;

namespace Infrastructure.Persistence;

public sealed class EfRepository<T>(ApplicationDbContext ctx) : IRepository<T> where T : class
{
	public IQueryable<T> Query(bool tracked = false) =>
		tracked ? ctx.Set<T>() : ctx.Set<T>().AsNoTracking();

	public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
		ctx.Set<T>().FindAsync([id], ct).AsTask();

	public Task AddAsync(T entity, CancellationToken ct = default) =>
		ctx.Set<T>().AddAsync(entity, ct).AsTask();

	public void Update(T entity) => ctx.Set<T>().Update(entity);
	public void SoftDelete(T entity) => ctx.Set<T>().Remove(entity);
	public async Task<IPaginate<T>> GetPageListAsync(
		PageRequest request,
		Expression<Func<T, bool>>? expression = null,
		Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null,
		Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
		bool enableTracking = true,
		bool splitQuery = false,
		CancellationToken ct = default)
	{
		IQueryable<T> query = enableTracking
			? ctx.Set<T>()
			: ctx.Set<T>().AsNoTracking();

		if (splitQuery)
			query = query.AsSplitQuery();
		if (expression is not null)
			query = query.Where(expression);
		if(include is not null)
			query = include(query);
		if(orderBy is not null)
			query = orderBy(query);

		return await query.ToPaginateAsync(
			ct,
			request.Page,
			request.PageSize,
			request.From
		);
	}
	public Task<bool> ExistsAsync(Expression<Func<T, bool>>? expression = null, CancellationToken ct = default) =>
		expression is null
			? Query().AnyAsync(ct)
			: Query().AnyAsync(expression, ct);

	public Task<int> SaveChangesAsync(CancellationToken ct = default) => ctx.SaveChangesAsync(ct);
}
