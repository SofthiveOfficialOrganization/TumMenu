using Application.Abstractions;
using Domain.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence;

public sealed class EfRepository<T>(DbContext ctx) : IRepository<T> where T : class
{
	public IQueryable<T> Query(bool tracked = false) =>
		tracked ? ctx.Set<T>() : ctx.Set<T>().AsNoTracking();

	public Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
		ctx.Set<T>().FindAsync([id], ct).AsTask();

	public Task AddAsync(T entity, CancellationToken ct) =>
		ctx.Set<T>().AddAsync(entity, ct).AsTask();

	public void Update(T entity) => ctx.Set<T>().Update(entity);
	public void Remove(T entity) => ctx.Set<T>().Remove(entity);
}
