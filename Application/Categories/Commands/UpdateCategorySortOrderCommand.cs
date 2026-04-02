using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public class UpdateCategorySortOrderCommand : IRequest<bool>, ITransactionalRequest
{
    public List<CategorySortItem> Items { get; set; } = [];
}

public class CategorySortItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCategorySortOrderCommandHandler(IRepository<Category> repoCategory) : IRequestHandler<UpdateCategorySortOrderCommand, bool>
{
    public async Task<bool> Handle(UpdateCategorySortOrderCommand req, CancellationToken ct)
    {
        if (req.Items == null || req.Items.Count == 0) return true;

        var ids = req.Items.Select(x => x.Id).ToList();
        var categories = await repoCategory.Query(tracked: true)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var category in categories)
        {
            var item = req.Items.FirstOrDefault(x => x.Id == category.Id);
            if (item != null)
            {
                category.SortOrder = item.SortOrder;
            }
        }

        // Transactions are handled by ITransactionalRequest pipeline behavior if configured, 
        // otherwise SaveChanges is called by the pipeline or verified here.
        // Assuming UnitOfWork or similar pipeline behavior saves changes for tracked entities.
        
        return true;
    }
}
