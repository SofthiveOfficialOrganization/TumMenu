using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Commands;

public class CreateCategoryLibraryItemCommand : IRequest<CategoryLibraryItemDTO>, ITransactionalRequest
{
	public string Title { get; set; } = string.Empty;
	public string? Slug { get; set; }
	public string? Description { get; set; }
	public string? IconKey { get; set; }
}


public class CreateCategoryLibraryItemCommandValidator : AbstractValidator<CreateCategoryLibraryItemCommand>
{
	public CreateCategoryLibraryItemCommandValidator()
	{
		RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
		RuleFor(c => c.Slug)
			.MaximumLength(100).WithMessage("Şirket slug'ı en fazla 100 karakter olabilir.")
			.Matches("^[a-z0-9-]+$").WithMessage("Şirket slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
				.When(c => !string.IsNullOrWhiteSpace(c.Slug));
	}
}

public class CreateCategoryLibraryItemCommandHandler(
	IRepository<CategoryLibraryItem> repoCategoryLibItem,
	IMapper mapper
) : IRequestHandler<CreateCategoryLibraryItemCommand, CategoryLibraryItemDTO>
{
	public async Task<CategoryLibraryItemDTO> Handle(CreateCategoryLibraryItemCommand req, CancellationToken ct)
	{
		var effectiveSlug = SlugHelper.Slugify(
			string.IsNullOrWhiteSpace(req.Slug) ? req.Title : req.Slug
		);

		var exists = await repoCategoryLibItem.Query().AnyAsync(x => x.Slug == effectiveSlug, ct);

		if(exists)
			throw new AlreadyExistsAppException(
				$"'{effectiveSlug}' slug'ına sahip bir kategori library item zaten var."
			);

		var entity = mapper.Map<CategoryLibraryItem>(req);
		entity.Slug = effectiveSlug;

		await repoCategoryLibItem.AddAsync(entity, ct);
		return mapper.Map<CategoryLibraryItemDTO>(entity);
	}
}
