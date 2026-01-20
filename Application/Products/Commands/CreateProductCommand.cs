using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Products.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.Products.Commands;

public sealed record CreateProductCommand(
	string Title,
	string? Slug,
	string? Description,
	decimal BasePrice,
	bool IsActive,
	bool? IsVegan,
	bool? IsVegetarian,
	int? EstimatedPreparationTimeInMinutes,
	int SortOrder,
	string? Allergens,
	Guid CategoryId
) : IRequest<ProductDTO>, ITransactionalRequest;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
	public CreateProductCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty()
			.MaximumLength(200);
		RuleFor(x => x.Slug)
			.MaximumLength(200);
		RuleFor(x => x.Description)
			.MaximumLength(1000);
		RuleFor(x => x.BasePrice)
			.GreaterThanOrEqualTo(0);
		RuleFor(x => x.SortOrder)
			.GreaterThanOrEqualTo(0);
		RuleFor(x => x.CategoryId)
			.NotEmpty();
	}
}

public class CreateProductCommandMappingProfile(
	IRepository<Product> repoProduct,
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<CreateProductCommand, ProductDTO>
{
	public async Task<ProductDTO> Handle(CreateProductCommand req, CancellationToken ct)
	{
		bool categoryExists = await repoCategory.ExistsAsync(r => r.Id == req.CategoryId, ct);
		if(!categoryExists)
			throw new UnprocessableAppException($"Ürünün ekleneceği kategori bulunamadı.");

		var product = mapper.Map<Product>(req);
		await repoProduct.AddAsync(product, ct);
		return mapper.Map<ProductDTO>(product);
	}
}