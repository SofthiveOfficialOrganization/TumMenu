using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Products.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Commands;

public class UpdateProductCommand : IRequest<ProductDTO>, ITransactionalRequest
{
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
	public IReadOnlyList<Guid> TagIds { get; set; } = [];
}

public class UpdateProductCommandHandler(
	IRepository<Product> repoProduct,
	IRepository<Tag> repoTag,
	IMapper mapper
) : IRequestHandler<UpdateProductCommand, ProductDTO>
{
	public async Task<ProductDTO> Handle(UpdateProductCommand req, CancellationToken ct)
	{
		var product = await repoProduct.Query()
			.Include(p => p.ProductTags)
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

		repoProduct.Update(product!);
		var productDTO = mapper.Map<ProductDTO>(product!);
		return productDTO;
	}
}
