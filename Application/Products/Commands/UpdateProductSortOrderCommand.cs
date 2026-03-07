using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Commands;

public class UpdateProductSortOrderCommand : IRequest<bool>, ITransactionalRequest
{
    public List<ProductSortItem> Items { get; set; } = [];
}

public class ProductSortItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateProductSortOrderHandler(IRepository<Product> repoProduct) : IRequestHandler<UpdateProductSortOrderCommand, bool>
{
    public async Task<bool> Handle(UpdateProductSortOrderCommand req, CancellationToken ct)
    {
        if (req.Items == null || req.Items.Count == 0) return true;

        var ids = req.Items.Select(x => x.Id).ToList();
        var products = await repoProduct.Query(tracked: true)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var product in products)
        {
            var item = req.Items.FirstOrDefault(x => x.Id == product.Id);
            if (item != null)
            {
                product.SortOrder = item.SortOrder;
            }
        }

        return true;
    }
}
