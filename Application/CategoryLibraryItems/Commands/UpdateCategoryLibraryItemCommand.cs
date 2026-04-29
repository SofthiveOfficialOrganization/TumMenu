

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

public class UpdateCategoryLibraryItemCommand : IRequest<CategoryLibraryItemDTO>, ITransactionalRequest
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? IconKey { get; set; }
}

public class UpdateCategoryLibraryItemCommandValidator : AbstractValidator<UpdateCategoryLibraryItemCommand>
{
	public UpdateCategoryLibraryItemCommandValidator()
	{
		RuleFor(x => x.Id).NotEmpty();
		RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
		RuleFor(c => c.Slug)
			.MaximumLength(100).WithMessage("Şirket slug'ı en fazla 100 karakter olabilir.")
			.Matches("^[a-z0-9-]+$").WithMessage("Şirket slug'ı sadece küçük harf, rakam ve tire (-) karakterlerinden oluşabilir.")
				.When(c => !string.IsNullOrWhiteSpace(c.Slug));
	}
}
public class UpdateCategoryLibraryItemCommandHandler(
	IRepository<CategoryLibraryItem> repoCategoryLibItem,
	IMapper mapper
) : IRequestHandler<UpdateCategoryLibraryItemCommand, CategoryLibraryItemDTO>
{
	public async Task<CategoryLibraryItemDTO> Handle(UpdateCategoryLibraryItemCommand req, CancellationToken ct)
	{
		var item = await repoCategoryLibItem.Query()
			.FirstOrDefaultAsync(x => x.Id == req.Id, ct);

		item = item.EnsureFound("Kategori library item bulunamadı.");

		var effectiveSlug = SlugHelper.Slugify(
			string.IsNullOrWhiteSpace(req.Slug) ? req.Title : req.Slug
		);

		var exists = await repoCategoryLibItem.Query()
			.AnyAsync(x => x.Slug == effectiveSlug && x.Id != req.Id, ct);

		if(exists)
			throw new AlreadyExistsAppException(
				$"'{effectiveSlug}' slug'ına sahip bir kategori library item zaten var. Farklı bir slug girin."
			);

		mapper.Map(req, item);
		item.Slug = effectiveSlug;

		repoCategoryLibItem.Update(item);

		return mapper.Map<CategoryLibraryItemDTO>(item);
	}
}
