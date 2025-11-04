using Application.Abstractions;
using Application.Abstractions.Repositories;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System.Globalization;
using System.Text;

namespace Application.Categories;

// DTO
public record CategoryDto(Guid Id, Guid MenuId, string Name, string Slug, int SortOrder);

// Command
public record CreateCategoryCommand(Guid MenuId, string Name, int SortOrder) : IRequest<CategoryDto>;

// Validator
public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
	public CreateCategoryValidator()
	{
		RuleFor(x => x.MenuId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
	}
}

// Handler
public class CreateCategoryHandler(ICategoryRepository repo, IUnitOfWork uow, IMapper mapper)
	: IRequestHandler<CreateCategoryCommand, CategoryDto>
{
	public async Task<CategoryDto> Handle(CreateCategoryCommand req, CancellationToken ct)
	{
		if(await repo.ExistsByNameAsync(req.MenuId, req.Name, ct))
			throw new InvalidOperationException("Bu menüde aynı isimde bir kategori zaten var.");

		var entity = mapper.Map<Category>(req);
		await repo.AddAsync(entity, ct);
		await uow.SaveChangesAsync(ct);
		return mapper.Map<CategoryDto>(entity);
	}
}
