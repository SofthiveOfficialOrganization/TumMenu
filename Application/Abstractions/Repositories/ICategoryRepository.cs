using Domain.Entities;

namespace Application.Abstractions.Repositories
{
	public interface ICategoryRepository
	{
		Task<bool> ExistsByNameAsync(Guid menuId, string name, CancellationToken ct);
		Task AddAsync(Category category, CancellationToken ct);
		Task<List<Category>> GetByMenuAsync(Guid menuId, CancellationToken ct);
		Task<Category> GetByIdAsync(Guid id, CancellationToken ct);
	}
}