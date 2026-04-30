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

public class CreateProductCommand : IRequest<ProductDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Ürün oluşturuldu";
	public string Title { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string? Description { get; set; }
	public decimal BasePrice { get; set; }
	public bool IsActive { get; set; }
	public bool? IsVegan { get; set; }
	public bool? IsVegetarian { get; set; }
	public int? EstimatedPreparationTimeInMinutes { get; set; }
	public int SortOrder { get; set; }
	public string? Allergens { get; set; }
	public Guid CategoryId { get; set; }
}

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
	}
}

public class CreateProductCommandHandler(
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

		// Auto-generate slug from Title if not provided
		var slug = string.IsNullOrWhiteSpace(req.Slug)
			? GenerateSlug(req.Title)
			: req.Slug;

		var product = mapper.Map<Product>(req);
		product.Slug = slug;
		await repoProduct.AddAsync(product, ct);
		return mapper.Map<ProductDTO>(product);
	}

	private static string GenerateSlug(string title)
	{
		var map = new Dictionary<char, string>
		{
			{'ğ',"g"},{'ü',"u"},{'ş',"s"},{'ı',"i"},{'ö',"o"},{'ç',"c"},
			{'Ğ',"g"},{'Ü',"u"},{'Ş',"s"},{'İ',"i"},{'I',"i"},{'Ö',"o"},{'Ç',"c"}
		};

		var sb = new System.Text.StringBuilder();
		foreach (var c in title.ToLower())
			sb.Append(map.TryGetValue(c, out var r) ? r : c.ToString());

		var result = sb.ToString()
			.Normalize(System.Text.NormalizationForm.FormD);
		result = System.Text.RegularExpressions.Regex.Replace(result, @"[^\u0000-\u007F]", "");
		result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9\s\-]", "");
		result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", "-");
		result = System.Text.RegularExpressions.Regex.Replace(result, @"-+", "-").Trim('-');

		if (result.Length > 30)
			result = result.Substring(0, 30).TrimEnd('-');

		return result;
	}
}
