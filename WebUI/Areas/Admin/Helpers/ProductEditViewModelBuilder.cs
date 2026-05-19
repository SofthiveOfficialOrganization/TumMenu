using Application.Products.Commands;
using Application.Products.DTOs;
using Application.Products.Queries;
using MediatR;

namespace WebUI.Areas.Admin.Helpers;

public static class ProductEditViewModelBuilder
{
	public static ProductDTO Merge(UpdateProductCommand req, ProductDTO existing)
	{
		existing.Title = req.Title;
		existing.Description = req.Description;
		existing.CategoryId = req.CategoryId;
		existing.BasePrice = req.BasePrice;
		existing.SortOrder = req.SortOrder;
		existing.IsActive = req.IsActive;
		existing.Allergens = req.Allergens;
		existing.IsVegan = req.IsVegan;
		existing.IsVegetarian = req.IsVegetarian;
		existing.EstimatedPreparationTimeInMinutes = req.EstimatedPreparationTimeInMinutes;
		existing.Prices = req.Prices
			.Where(p => !string.IsNullOrWhiteSpace(p.Size) || p.Price.HasValue)
			.Select(p => new ProductPriceDTO
			{
				Size = p.Size?.Trim(),
				Price = p.Price ?? 0
			})
			.ToList();
		return existing;
	}

	public static async Task<ProductDTO> BuildAsync(IMediator mediator, UpdateProductCommand req, CancellationToken ct)
	{
		var existing = await mediator.Send(new GetProductByIdQuery(req.Id), ct);
		return Merge(req, existing);
	}
}
