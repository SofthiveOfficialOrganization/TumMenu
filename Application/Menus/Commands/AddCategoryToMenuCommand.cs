using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class AddCategoryToMenuCommand : IRequest<CategoryDTO>, ITransactionalRequest
{
	public Guid MenuId { get; set; }
	public Guid CategoryLibraryItemId { get; set; }
	public Guid? ParentId { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; } = true;
}

public class AddCategoryToMenuCommandValidator : AbstractValidator<AddCategoryToMenuCommand>
{
	public AddCategoryToMenuCommandValidator()
	{
		RuleFor(x => x.MenuId).NotEmpty();
		RuleFor(x => x.CategoryLibraryItemId).NotEmpty();
	}
}

public class AddCategoryToMenuCommandHandler(
	IRepository<Menu> repoMenu,
	IRepository<CategoryLibraryItem> repoLibraryItem,
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<AddCategoryToMenuCommand, CategoryDTO>
{
	public async Task<CategoryDTO> Handle(AddCategoryToMenuCommand req, CancellationToken ct)
	{
		var menuExists = await repoMenu.Query().AnyAsync(x => x.Id == req.MenuId, ct);
		if (!menuExists)
			throw new NotFoundAppException("Menü bulunamadı.");

		var libraryItem = await repoLibraryItem.Query().FirstOrDefaultAsync(x => x.Id == req.CategoryLibraryItemId, ct);
		if (libraryItem == null)
			throw new NotFoundAppException("Kategori kütüphane öğesi bulunamadı.");

		var linkExists = await repoCategory.Query()
			.AnyAsync(x => x.MenuId == req.MenuId && x.CategoryLibraryItemId == req.CategoryLibraryItemId, ct);
		
		if (linkExists)
			throw new AlreadyExistsAppException("Bu kategori zaten menüde ekli.");

		// Validate ParentId
		if (req.ParentId.HasValue)
		{
			var parentExists = await repoCategory.Query()
				.AnyAsync(x => x.Id == req.ParentId.Value && x.MenuId == req.MenuId, ct);
			if (!parentExists)
				throw new NotFoundAppException("Üst kategori bulunamadı veya bu menüye ait değil.");
		}

		var category = new Category
		{
			MenuId = req.MenuId,
			CategoryLibraryItemId = req.CategoryLibraryItemId,
			ParentId = req.ParentId,
			SortOrder = req.SortOrder,
			IsActive = req.IsActive
		};

		await repoCategory.AddAsync(category, ct);
		
		// We need to load the Library Item to map it back to DTO
		category.CategoryLibraryItem = libraryItem;

		return mapper.Map<CategoryDTO>(category);
	}
}
