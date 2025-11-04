using Application.Abstractions.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoryRepository(ApplicationDbContext db) : BaseRepository<Category>(db), ICategoryRepository
{
	public Task<bool> ExistsByNameAsync(Guid menuId, string name, CancellationToken ct)
		=> _set.AnyAsync(x => x.MenuId == menuId && x.Name == name, ct);

	public Task<List<Category>> GetByMenuAsync(Guid menuId, CancellationToken ct)
		=> _set.Where(x => x.MenuId == menuId)
			   .OrderBy(x => x.SortOrder)
			   .ToListAsync(ct);
}
