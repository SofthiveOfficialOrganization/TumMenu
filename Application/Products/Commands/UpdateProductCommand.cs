using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Products.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Commands;

public class UpdateProductCommand : IRequest<ProductDTO>, ITransactionalRequest, IEntityAuditableCommand
{
	public string ActionName => "Ürün güncellendi";
	public Guid EntityId => Id;
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid CategoryId { get; set; }
	public decimal BasePrice { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public string? Allergens { get; set; }
	public bool? IsVegan { get; set; }
	public bool? IsVegetarian { get; set; }
	public int? EstimatedPreparationTimeInMinutes { get; set; }
	public List<ProductPriceInputDTO> Prices { get; set; } = [];
	public IReadOnlyList<Guid> TagIds { get; set; } = [];
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
	public UpdateProductCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty()
			.MaximumLength(200);
		RuleFor(x => x.Description)
			.MaximumLength(1000);
		RuleFor(x => x.BasePrice)
			.GreaterThanOrEqualTo(0)
			.LessThanOrEqualTo(9999);
		RuleFor(x => x.EstimatedPreparationTimeInMinutes)
			.LessThanOrEqualTo(99)
			.When(x => x.EstimatedPreparationTimeInMinutes.HasValue);
		RuleFor(x => x.SortOrder)
			.GreaterThanOrEqualTo(0);
		RuleFor(x => x.CategoryId)
			.NotEmpty();
		RuleFor(x => x.Allergens)
			.MaximumLength(200)
			.WithMessage("Alerjenler alanı en fazla 200 karakter olabilir.");
		RuleForEach(x => x.Prices).ChildRules(price =>
		{
			price.RuleFor(x => x.Size)
				.MaximumLength(100);
			price.RuleFor(x => x.Price)
				.GreaterThanOrEqualTo(0)
				.LessThanOrEqualTo(9999)
				.When(x => x.Price.HasValue);
		});
	}
}

public class UpdateProductCommandHandler(
	IRepository<Product> repoProduct,
	IRepository<Tag> repoTag,
	IMapper mapper
) : IRequestHandler<UpdateProductCommand, ProductDTO>
{
	public async Task<ProductDTO> Handle(UpdateProductCommand req, CancellationToken ct)
	{
		var product = await repoProduct.Query(tracked: true)
			.Include(p => p.ProductTags)
			.Include(p => p.Prices)
			.FirstOrDefaultAsync(p => p.Id == req.Id, ct)
			.EnsureFound("Ürün bulunamadı."); mapper.Map(req, product);

		var newTags = (req.TagIds ?? []).Distinct().ToHashSet();
		var existingTags = product!.ProductTags.Select(pt => pt.TagId).ToHashSet();

		var tagsToAdd = newTags.Except(existingTags).ToList();
		var tagsToRemove = existingTags.Except(newTags).ToList();

		if(tagsToAdd.Count != 0)
		{
			var tags = await repoTag.Query()
				.Where(t => tagsToAdd.Contains(t.Id))
				.ToListAsync(ct);
			foreach(var tag in tags)
			{
				product.ProductTags.Add(new ProductTag
				{
					ProductId = product.Id,
					TagId = tag.Id
				});
			}
		}
		if(tagsToRemove.Count != 0)
		{
			product.ProductTags.RemoveWhere(pt => tagsToRemove.Contains(pt.TagId));
		}

		product.Prices.Clear();
		foreach(var price in req.Prices.Where(p => !string.IsNullOrWhiteSpace(p.Size) && p.Price is >= 0 and <= 9999))
		{
			product.Prices.Add(new ProductPrice
			{
				ProductId = product.Id,
				Size = price.Size!.Trim(),
				Price = price.Price!.Value
			});
		}

		var productDTO = mapper.Map<ProductDTO>(product!);
		return productDTO;
	}
}
