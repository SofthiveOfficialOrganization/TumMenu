using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Products.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Commands;

public sealed record UpdateProductCommand(
	Guid Id,
	string Name,
	string? Description,
	Guid CategoryId,
	decimal BasePrice,
	int SortOrder,
	bool IsActive,
	string? Allergens,
	bool? IsVegan,
	bool? IsVegetarian,
	int? EstimatedPreparationTimeInMinutes
) : IRequest<ProductDTO>, ITransactionalRequest;

public class UpdateProductCommandHandler(
	IRepository<Product> repoProduct,
	IMapper mapper
) : IRequestHandler<UpdateProductCommand, ProductDTO>
{
	public async Task<ProductDTO> Handle(UpdateProductCommand req, CancellationToken ct)
	{
		var product = (await repoProduct.GetByIdAsync(req.Id, ct)).EnsureFound("Ürün bulunamadı.");
		mapper.Map(req, product);
		repoProduct.Update(product);
		return mapper.Map<ProductDTO>(product);
	}
}